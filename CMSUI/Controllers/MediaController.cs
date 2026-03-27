using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class MediaController : Controller
    {
        private readonly IConfiguration _config;
        private const long MaxBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] TiposPermitidos =
            ["image/jpeg", "image/png", "image/webp", "image/svg+xml", "image/gif"];

        public MediaController(IConfiguration config) => _config = config;

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        private BlobContainerClient GetContainer()
        {
            var connStr = _config["AzureStorage:ConnectionString"]!;
            var container = _config["AzureStorage:Container"] ?? "cms-imagens";
            return new BlobContainerClient(connStr, container);
        }

        private string ResolverTenant(bool acessoTotal, string? claimAppId, string? queryAppId)
        {
            if (acessoTotal && !string.IsNullOrEmpty(queryAppId)) return queryAppId;
            return claimAppId ?? "global";
        }

        [HttpGet]
        public async Task<IActionResult> Listar([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var tenant = ResolverTenant(acessoTotal, claimAppId, aplicacaoid);
            var prefix = tenant + "/";
            var container = GetContainer();
            var result = new List<object>();
            await foreach (var blob in container.GetBlobsAsync(prefix: prefix))
            {
                result.Add(new
                {
                    nome = blob.Name[prefix.Length..],
                    blobName = blob.Name,
                    url = $"{container.Uri}/{blob.Name}",
                    tamanho = blob.Properties.ContentLength ?? 0,
                    data = blob.Properties.CreatedOn
                });
            }
            return Ok(result);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromQuery] string? aplicacaoid, IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return BadRequest(new { erro = "Nenhum arquivo enviado." });
            if (arquivo.Length > MaxBytes)
                return BadRequest(new { erro = "Arquivo muito grande. Limite: 5 MB." });
            if (!TiposPermitidos.Contains(arquivo.ContentType))
                return BadRequest(new { erro = "Tipo não permitido. Use JPG, PNG, WebP, SVG ou GIF." });

            var (acessoTotal, claimAppId) = UserContext();
            var tenant = ResolverTenant(acessoTotal, claimAppId, aplicacaoid);
            var ext = Path.GetExtension(arquivo.FileName);
            var blobName = $"{tenant}/{Guid.NewGuid()}{ext}";

            var container = GetContainer();
            var blob = container.GetBlobClient(blobName);
            var opts = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = arquivo.ContentType }
            };
            using var stream = arquivo.OpenReadStream();
            await blob.UploadAsync(stream, opts);

            return Ok(new { url = blob.Uri.ToString(), blobName, nome = Path.GetFileName(blobName) });
        }

        [HttpDelete]
        public async Task<IActionResult> Deletar([FromQuery] string blobName)
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (!acessoTotal && !blobName.StartsWith(claimAppId + "/"))
                return Forbid();
            var container = GetContainer();
            await container.GetBlobClient(blobName).DeleteIfExistsAsync();
            return Ok();
        }
    }
}
