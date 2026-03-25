using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSUI.Models;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class AplicacaosController : Controller
    {
        private readonly CmsxDbContext _context;
        public AplicacaosController(CmsxDbContext context) { _context = context; }

        private (bool acessoTotal, string? aplicacaoid) UserContext() =>
            (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

        [HttpGet]
        public IEnumerable<Aplicacao> Get()
        {
            var (acessoTotal, claimAppId) = UserContext();
            if (acessoTotal)
                return _context.Aplicacaos.OrderBy(a => a.Nome).ToArray();

            return _context.Aplicacaos
                .Where(a => a.Aplicacaoid == claimAppId)
                .ToArray();
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _context.Aplicacaos.Find(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();
            return Ok(item);
        }

        public class NovaAplicacaoDto
        {
            public string? Nome { get; set; }
            public string? Url { get; set; }
            public string? Mailuser { get; set; }
            public string? Mailpassword { get; set; }
            public string? Mailserver { get; set; }
            public int? Mailport { get; set; }
            public bool? Issecure { get; set; }
            public string? Pagsegurotoken { get; set; }
            public string? Layoutchoose { get; set; }
            public string? Ogleadsense { get; set; }
            public string? Header { get; set; }
            public string? Pagefacebook { get; set; }
            public string? Pageinstagram { get; set; }
            public string? Pagetwitter { get; set; }
            public string? Pagelinkedin { get; set; }
            public string? Pagepinterest { get; set; }
            public string? Pageflicker { get; set; }
            public string? Idusuarioinicio { get; set; }
            public string? Telefone { get; set; }
            public string? Endereco { get; set; }
            public string? Descricao { get; set; }
        }

        // Criação de aplicação: somente admin
        [HttpPost]
        public IActionResult Post([FromBody] NovaAplicacaoDto dto)
        {
            var (acessoTotal, _) = UserContext();
            if (!acessoTotal) return Forbid();

            var item = new Aplicacao
            {
                Aplicacaoid     = Guid.NewGuid().ToString(),
                Nome            = dto.Nome,
                Url             = dto.Url,
                Mailuser        = dto.Mailuser,
                Mailpassword    = dto.Mailpassword,
                Mailserver      = dto.Mailserver,
                Mailport        = dto.Mailport,
                Issecure        = dto.Issecure.HasValue ? (byte?)(dto.Issecure.Value ? 1 : 0) : null,
                Pagsegurotoken  = dto.Pagsegurotoken,
                Layoutchoose    = string.IsNullOrWhiteSpace(dto.Layoutchoose) ? "_Layout.cshtml" : dto.Layoutchoose,
                Ogleadsense     = dto.Ogleadsense,
                Header          = dto.Header,
                Pagefacebook    = dto.Pagefacebook,
                Pageinstagram   = dto.Pageinstagram,
                Pagetwitter     = dto.Pagetwitter,
                Pagelinkedin    = dto.Pagelinkedin,
                Pagepinterest   = dto.Pagepinterest,
                Pageflicker     = dto.Pageflicker,
                Idusuarioinicio = dto.Idusuarioinicio,
                Datainicio      = DateTime.Now,
                Isactive        = true,
                Telefone        = dto.Telefone,
                Endereco        = dto.Endereco,
                Descricao       = dto.Descricao
            };
            _context.Aplicacaos.Add(item);

            // Auto-cria área Home com template padrão (se existir)
            var templatePadrao = _context.LayoutTemplates
                .FirstOrDefault(t => t.Tipo == "home" && t.Padrao);

            _context.Areas.Add(new Area
            {
                Areaid       = Guid.NewGuid().ToString(),
                Aplicacaoid  = item.Aplicacaoid,
                Nome         = "Home",
                Url          = "home",
                Posicao      = 1,
                Layout       = templatePadrao?.Layout ?? "{\"blocos\":[]}"
            });

            _context.SaveChanges();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public IActionResult Put(string id, [FromBody] NovaAplicacaoDto dto)
        {
            var (acessoTotal, claimAppId) = UserContext();
            var item = _context.Aplicacaos.Find(id);
            if (item == null) return NotFound();
            if (!acessoTotal && item.Aplicacaoid != claimAppId) return Forbid();

            item.Nome           = dto.Nome;
            item.Url            = dto.Url;
            item.Mailuser       = dto.Mailuser;
            item.Mailpassword   = dto.Mailpassword;
            item.Mailserver     = dto.Mailserver;
            item.Mailport       = dto.Mailport;
            item.Issecure       = dto.Issecure.HasValue ? (byte?)(dto.Issecure.Value ? 1 : 0) : null;
            item.Pagsegurotoken = dto.Pagsegurotoken;
            item.Layoutchoose   = string.IsNullOrWhiteSpace(dto.Layoutchoose) ? "_Layout.cshtml" : dto.Layoutchoose;
            item.Ogleadsense    = dto.Ogleadsense;
            item.Header         = dto.Header;
            item.Pagefacebook   = dto.Pagefacebook;
            item.Pageinstagram  = dto.Pageinstagram;
            item.Pagetwitter    = dto.Pagetwitter;
            item.Pagelinkedin   = dto.Pagelinkedin;
            item.Pagepinterest  = dto.Pagepinterest;
            item.Pageflicker    = dto.Pageflicker;
            item.Telefone       = dto.Telefone;
            item.Endereco       = dto.Endereco;
            item.Descricao      = dto.Descricao;
            _context.SaveChanges();
            return Ok(item);
        }

        // Ativação/desativação: somente admin
        [HttpPatch("{id}/status")]
        public IActionResult PatchStatus(string id, [FromBody] bool ativo)
        {
            var (acessoTotal, _) = UserContext();
            if (!acessoTotal) return Forbid();

            var item = _context.Aplicacaos.Find(id);
            if (item == null) return NotFound();
            item.Isactive  = ativo;
            item.Datafinal = ativo ? null : DateTime.Now;
            _context.SaveChanges();
            return Ok(item);
        }

        // Exclusão: somente admin
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            var (acessoTotal, _) = UserContext();
            if (!acessoTotal) return Forbid();

            var item = _context.Aplicacaos.Find(id);
            if (item == null) return NotFound();
            _context.Aplicacaos.Remove(item);
            _context.SaveChanges();
            return Ok();
        }
    }
}
