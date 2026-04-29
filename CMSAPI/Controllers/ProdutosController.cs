using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using CMSXData.Models;
using CMSAPI.Services;
using ICMSX;

namespace CMSAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ProdutosController : Controller
    {
        private readonly CmsxDbContext _context;
        private readonly IAgentIAFactory _agentFactory;
        private readonly IWebHostEnvironment _env;

        public ProdutosController(CmsxDbContext context, IAgentIAFactory agentFactory, IWebHostEnvironment env)
        {
            _context = context;
            _agentFactory = agentFactory;
            _env = env;
        }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        // ── CRUD Produto ─────────────────────────────────────────────────────

        [HttpGet]
        public IEnumerable<Produto> Get([FromQuery] string? aplicacaoid = null)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var q = _context.Produtos.AsQueryable();
            if (acessoTotal)
            {
                if (!string.IsNullOrEmpty(aplicacaoid))
                    q = q.Where(p => p.Aplicacaoid == aplicacaoid);
            }
            else
            {
                q = q.Where(p => p.Aplicacaoid == claimAppId);
            }
            return q.OrderBy(p => p.Nome).ToArray();
        }

        public class NovoProdutoDto
        {
            public string? Nome { get; set; }
            public string? Descricao { get; set; }
            public string? Descricacurta { get; set; }
            public string? Detalhetecnico { get; set; }
            public string? Pagsegurokey { get; set; }
            public decimal? Valor { get; set; }
            public string Sku { get; set; } = "";
            public int? Tipo { get; set; }
            public int? Destaque { get; set; }
            public string? Cateriaid { get; set; }
            public string? Aplicacaoid { get; set; }
            public string? UnidadeVenda { get; set; }
        }

        [HttpPost]
        public IActionResult Post([FromBody] NovoProdutoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = new Produto
            {
                Produtoid      = Guid.NewGuid().ToString(),
                Sku            = string.IsNullOrWhiteSpace(dto.Sku) ? Guid.NewGuid().ToString()[..8] : dto.Sku,
                Nome           = dto.Nome,
                Descricao      = dto.Descricao,
                Descricacurta  = dto.Descricacurta,
                Detalhetecnico = dto.Detalhetecnico,
                Pagsegurokey   = dto.Pagsegurokey,
                Valor          = dto.Valor,
                Tipo           = dto.Tipo,
                Destaque       = dto.Destaque,
                Cateriaid      = dto.Cateriaid,
                Aplicacaoid    = acessoTotal ? dto.Aplicacaoid : claimAppId,
                UnidadeVenda   = dto.UnidadeVenda,
                Datainicio     = DateTime.Now
            };
            _context.Produtos.Add(item);
            _context.SaveChanges();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovoProdutoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            item.Nome           = dto.Nome;
            item.Descricao      = dto.Descricao;
            item.Descricacurta  = dto.Descricacurta;
            item.Detalhetecnico = dto.Detalhetecnico;
            item.Pagsegurokey   = dto.Pagsegurokey;
            item.Valor          = dto.Valor;
            item.Tipo           = dto.Tipo;
            item.Destaque       = dto.Destaque;
            item.Cateriaid      = dto.Cateriaid;
            item.UnidadeVenda   = dto.UnidadeVenda;
            _context.SaveChanges();
            return Ok(item);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            _context.Produtos.Remove(item);
            _context.SaveChanges();
            return Ok();
        }

        // ── Atributos ────────────────────────────────────────────────────────

        private record OpcaoResponse(string Opcaoid, string? Nome, string? Descricao, int Qtd, int? Estoque, decimal? ValorAdicional);
        private record AtributoResponse(Guid Atributoid, string Nome, string Descricao, int? Ordem, Guid? ParentAtributoId,
            decimal? ValorAdicional, List<OpcaoResponse> Opcoes, List<AtributoResponse> Filhos);

        private List<AtributoResponse> BuildAtributoTree(
            List<Atributo> todos,
            Dictionary<Guid, List<OpcaoResponse>> opcoesPorAtributo,
            Guid? parentId)
        {
            return todos
                .Where(a => a.ParentAtributoId == parentId)
                .OrderBy(a => a.Ordem ?? 0)
                .Select(a =>
                {
                    var filhos = BuildAtributoTree(todos, opcoesPorAtributo, a.Atributoid);
                    var opcoes = filhos.Count == 0 && opcoesPorAtributo.TryGetValue(a.Atributoid, out var ops) ? ops : [];
                    return new AtributoResponse(a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.ValorAdicional, opcoes, filhos);
                })
                .ToList();
        }

        [HttpGet("{id}/atributos")]
        public IActionResult GetAtributos(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var todosAtribs = _context.Atributos
                .Where(a => a.Produtoid == id || _context.Atributos
                    .Where(r => r.Produtoid == id)
                    .Select(r => r.Atributoid)
                    .Contains(a.ParentAtributoId!.Value))
                .ToList();

            // coleta todos descendentes iterativamente para cobrir N níveis
            var idsConhecidos = todosAtribs.Select(a => a.Atributoid).ToHashSet();
            bool achouMais;
            do
            {
                achouMais = false;
                var novos = _context.Atributos
                    .Where(a => a.ParentAtributoId.HasValue && idsConhecidos.Contains(a.ParentAtributoId.Value) && !idsConhecidos.Contains(a.Atributoid))
                    .ToList();
                if (novos.Count > 0)
                {
                    todosAtribs.AddRange(novos);
                    foreach (var n in novos) idsConhecidos.Add(n.Atributoid);
                    achouMais = true;
                }
            } while (achouMais);

            var opcoesPorAtributo = _context.Opcaos
                .Where(o => idsConhecidos.Contains(o.Atributoid))
                .ToList()
                .GroupBy(o => o.Atributoid)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(o => new OpcaoResponse(o.Opcaoid, o.Nome, o.Descricao, o.Qtd, o.Estoque, o.ValorAdicional)).ToList()
                );

            return Ok(BuildAtributoTree(todosAtribs, opcoesPorAtributo, null));
        }

        public class AtributoDto
        {
            public string Nome { get; set; } = "";
            public string? Descricao { get; set; }
            public Guid? ParentAtributoId { get; set; }
            public int? Ordem { get; set; }
            public decimal? ValorAdicional { get; set; }
        }

        [HttpPost("{id}/atributos")]
        public IActionResult PostAtributo(string id, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            if (dto.ParentAtributoId.HasValue)
            {
                var parent = _context.Atributos.FirstOrDefault(a => a.Atributoid == dto.ParentAtributoId.Value);
                if (parent == null) return BadRequest("ParentAtributoId não encontrado.");
            }

            var a = new Atributo
            {
                Atributoid      = Guid.NewGuid(),
                Produtoid       = dto.ParentAtributoId.HasValue ? null : id,
                ParentAtributoId = dto.ParentAtributoId,
                Nome            = dto.Nome,
                Descricao       = dto.Descricao ?? "",
                Ordem           = dto.Ordem,
                ValorAdicional  = dto.ValorAdicional
            };
            _context.Atributos.Add(a);
            _context.SaveChanges();
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpPut("{id}/atributos/{atributoid}")]
        public IActionResult PutAtributo(string id, Guid atributoid, [FromBody] AtributoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var a = _context.Atributos.FirstOrDefault(x => x.Atributoid == atributoid);
            if (a == null) return NotFound();

            if (dto.ParentAtributoId.HasValue && dto.ParentAtributoId != a.ParentAtributoId)
            {
                var parent = _context.Atributos.FirstOrDefault(x => x.Atributoid == dto.ParentAtributoId.Value);
                if (parent == null) return BadRequest("ParentAtributoId não encontrado.");
            }

            a.Nome            = dto.Nome;
            a.Descricao       = dto.Descricao ?? a.Descricao;
            a.Ordem           = dto.Ordem ?? a.Ordem;
            a.ParentAtributoId = dto.ParentAtributoId;
            a.ValorAdicional  = dto.ValorAdicional;
            _context.SaveChanges();
            return Ok(new { a.Atributoid, a.Nome, a.Descricao, a.Ordem, a.ParentAtributoId, a.Produtoid });
        }

        [HttpDelete("{id}/atributos/{atributoid}")]
        public IActionResult DeleteAtributo(string id, Guid atributoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var a = _context.Atributos.FirstOrDefault(x => x.Atributoid == atributoid);
            if (a == null) return NotFound();

            // coleta todos os descendentes
            var todosIds = new List<Guid> { atributoid };
            var queue = new Queue<Guid>();
            queue.Enqueue(atributoid);
            while (queue.Count > 0)
            {
                var pid = queue.Dequeue();
                var filhos = _context.Atributos
                    .Where(x => x.ParentAtributoId == pid)
                    .Select(x => x.Atributoid)
                    .ToList();
                foreach (var fid in filhos) { todosIds.Add(fid); queue.Enqueue(fid); }
            }

            var opcoes = _context.Opcaos.Where(o => todosIds.Contains(o.Atributoid)).ToList();
            _context.Opcaos.RemoveRange(opcoes);
            var atribs = _context.Atributos.Where(x => todosIds.Contains(x.Atributoid)).ToList();
            _context.Atributos.RemoveRange(atribs);
            _context.SaveChanges();
            return Ok();
        }

        // ── Opções ───────────────────────────────────────────────────────────

        public class OpcaoDto
        {
            public string? Nome { get; set; }
            public string? Descricao { get; set; }
            public int Qtd { get; set; }
            public int? Estoque { get; set; }
            public decimal? ValorAdicional { get; set; }
        }

        [HttpPost("{id}/atributos/{atributoid}/opcoes")]
        public IActionResult PostOpcao(string id, Guid atributoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = new Opcao
            {
                Opcaoid        = Guid.NewGuid().ToString(),
                Atributoid     = atributoid,
                Nome           = dto.Nome,
                Descricao      = dto.Descricao,
                Qtd            = dto.Qtd,
                Estoque        = dto.Estoque,
                ValorAdicional = dto.ValorAdicional
            };
            _context.Opcaos.Add(o);
            _context.SaveChanges();
            return Ok(o);
        }

        [HttpPut("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public IActionResult PutOpcao(string id, Guid atributoid, string opcaoid, [FromBody] OpcaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = _context.Opcaos.FirstOrDefault(x => x.Opcaoid == opcaoid && x.Atributoid == atributoid);
            if (o == null) return NotFound();

            o.Nome           = dto.Nome;
            o.Descricao      = dto.Descricao;
            o.Qtd            = dto.Qtd;
            o.Estoque        = dto.Estoque;
            o.ValorAdicional = dto.ValorAdicional;
            _context.SaveChanges();
            return Ok(o);
        }

        [HttpDelete("{id}/atributos/{atributoid}/opcoes/{opcaoid}")]
        public IActionResult DeleteOpcao(string id, Guid atributoid, string opcaoid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var o = _context.Opcaos.FirstOrDefault(x => x.Opcaoid == opcaoid && x.Atributoid == atributoid);
            if (o == null) return NotFound();
            _context.Opcaos.Remove(o);
            _context.SaveChanges();
            return Ok();
        }

        // ── Galeria de Imagens ────────────────────────────────────────────────

        [HttpGet("{id}/imagens")]
        public IActionResult GetImagens(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            return Ok(_context.Imagems.Where(i => i.Parentid == id && i.Tipoid == "produto").ToArray());
        }

        public class ImagemDto { public string? Url { get; set; } public string? Descricao { get; set; } }

        [HttpPost("{id}/imagens")]
        public IActionResult PostImagem(string id, [FromBody] ImagemDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var img = new Imagem
            {
                Imagemid  = Guid.NewGuid().ToString(),
                Parentid  = id,
                Tipoid    = "produto",
                Url       = dto.Url,
                Descricao = dto.Descricao
            };
            _context.Imagems.Add(img);
            _context.SaveChanges();
            return Ok(img);
        }

        // ── Geração de descrição via IA (visão) ──────────────────────────────

        [HttpPost("gerar-descricao")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GerarDescricao(
            [FromForm] IFormFile? arquivo,
            [FromForm] string? imageUrl,
            [FromForm] string? provedor,
            [FromForm] string? produtoid)
        {
            var (acessoTotal, claimAppId) = UserContext();

            byte[] imageBytes;
            string mimeType;
            string? imagemSalvaUrl = null;

            if (arquivo != null && arquivo.Length > 0)
            {
                using var ms = new MemoryStream();
                await arquivo.CopyToAsync(ms);
                imageBytes = ms.ToArray();
                mimeType = arquivo.ContentType ?? "image/jpeg";

                // Salva o arquivo em wwwroot/uploads/{aplicacaoid}/
                var appFolder = claimAppId ?? "geral";
                var uploadsPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", appFolder);
                Directory.CreateDirectory(uploadsPath);
                var ext = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{ext}";
                await System.IO.File.WriteAllBytesAsync(Path.Combine(uploadsPath, fileName), imageBytes);
                imagemSalvaUrl = $"/uploads/{appFolder}/{fileName}";

                // Adiciona à galeria se o produto já existe
                if (!string.IsNullOrEmpty(produtoid))
                {
                    var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == produtoid);
                    if (produto != null && (acessoTotal || produto.Aplicacaoid == claimAppId))
                    {
                        _context.Imagems.Add(new Imagem
                        {
                            Imagemid  = Guid.NewGuid().ToString(),
                            Parentid  = produtoid,
                            Tipoid    = "produto",
                            Url       = imagemSalvaUrl,
                            Descricao = "Gerado por IA"
                        });
                        _context.SaveChanges();
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                using var http = new HttpClient();
                imageBytes = await http.GetByteArrayAsync(imageUrl);
                var ext = Path.GetExtension(imageUrl).ToLowerInvariant().TrimStart('.');
                mimeType = ext switch
                {
                    "jpg" or "jpeg" => "image/jpeg",
                    "png"           => "image/png",
                    "webp"          => "image/webp",
                    "gif"           => "image/gif",
                    _               => "image/jpeg"
                };
                imagemSalvaUrl = imageUrl;
            }
            else
            {
                return BadRequest("Informe uma URL ou faça upload de uma imagem.");
            }

            var prompt = @"Analise esta imagem de produto e retorne APENAS um JSON válido, sem texto adicional:
{
  ""nome"": ""<nome do produto>"",
  ""descricacurta"": ""<descrição curta, máx 120 caracteres>"",
  ""descricao"": ""<descrição completa do produto>"",
  ""detalhetecnico"": ""<especificações técnicas, materiais, dimensões se visíveis>"",
  ""atributos"": [
    { ""nome"": ""<nome do atributo, ex: Cor, Tamanho>"", ""opcoes"": [{ ""nome"": ""<valor>"", ""estoque"": 0 }] }
  ]
}
Se não identificar variações ou atributos na imagem, retorne ""atributos"" como array vazio [].
Responda em português do Brasil.";

            try
            {
                var agente = _agentFactory.Criar(provedor);
                var raw = LimparMarkdown(await agente.GerarComImagemAsync(imageBytes, mimeType, prompt));
                var dados = JsonDocument.Parse(raw).RootElement;
                var atributos = dados.TryGetProperty("atributos", out var atrib) ? atrib : (JsonElement?)null;
                return Ok(new
                {
                    nome           = dados.GetProperty("nome").GetString(),
                    descricacurta  = dados.GetProperty("descricacurta").GetString(),
                    descricao      = dados.GetProperty("descricao").GetString(),
                    detalhetecnico = dados.GetProperty("detalhetecnico").GetString(),
                    imagemUrl      = imagemSalvaUrl,
                    atributos      = atributos?.ValueKind == JsonValueKind.Array ? atributos : null,
                    provedor       = agente.Provedor
                });
            }
            catch (JsonException ex)
            {
                return UnprocessableEntity(new { erro = "JSON inválido.", detalhe = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Erro ao chamar o agente IA: {ex.Message}");
            }
        }

        private static string LimparMarkdown(string texto)
        {
            var inicio = texto.IndexOf('{');
            var fim = texto.LastIndexOf('}');
            if (inicio >= 0 && fim > inicio)
                return texto[inicio..(fim + 1)];
            return texto.Trim();
        }

        [HttpDelete("{id}/imagens/{imagemid}")]
        public IActionResult DeleteImagem(string id, string imagemid)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var produto = _context.Produtos.FirstOrDefault(p => p.Produtoid == id);
            if (produto == null) return NotFound();
            if (!acessoTotal && produto.Aplicacaoid != claimAppId) return Forbid();

            var img = _context.Imagems.FirstOrDefault(i => i.Imagemid == imagemid && i.Parentid == id);
            if (img == null) return NotFound();
            _context.Imagems.Remove(img);
            _context.SaveChanges();
            return Ok();
        }
    }
}