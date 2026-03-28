using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CMSUI.Models;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly CmsxDbContext _context;

        public SiteController(CmsxDbContext context) => _context = context;

        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public IActionResult GetBySlug(string slug)
        {
            var app = _context.Aplicacaos.FirstOrDefault(a => a.Url == slug);
            return app == null ? NotFound(new { erro = $"Site '{slug}' não encontrado." }) : BuildSiteData(app);
        }

        [Authorize]
        [HttpGet("preview/{aplicacaoid}")]
        public IActionResult GetPreview(string aplicacaoid)
        {
            var app = _context.Aplicacaos.FirstOrDefault(a => a.Aplicacaoid == aplicacaoid);
            return app == null ? NotFound() : BuildSiteData(app);
        }

        private IActionResult BuildSiteData(Aplicacao app)
        {
            var areas = _context.Areas
                .Where(a => a.Aplicacaoid == app.Aplicacaoid)
                .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
                .ToList();

            var areasResult = areas.Select(area =>
            {
                var blocos = new List<object>();
                if (!string.IsNullOrEmpty(area.Layout) && area.Layout != "{\"blocos\":[]}")
                {
                    try
                    {
                        var doc = JsonDocument.Parse(area.Layout);
                        foreach (var b in doc.RootElement.GetProperty("blocos").EnumerateArray())
                        {
                            var tipo = b.GetProperty("tipo").GetString() ?? "";
                            var configRaw = b.GetProperty("config").GetRawText();
                            var config = JsonDocument.Parse(configRaw).RootElement;
                            var coluna = b.TryGetProperty("coluna", out var colunaEl) ? colunaEl.GetString() : null;
                            var dados = EnriquecerBloco(tipo, config, app.Aplicacaoid!);
                            blocos.Add(new { tipo, config = JsonSerializer.Deserialize<object>(configRaw), coluna, dados });
                        }
                    }
                    catch { }
                }
                return new { area.Areaid, area.Nome, area.Url, TemLayout = blocos.Count > 0, blocos };
            }).ToList();

            return Ok(new { app.Aplicacaoid, app.Nome, app.Url, app.Header, areas = areasResult });
        }

        private object? EnriquecerBloco(string tipo, JsonElement config, string aplicacaoid)
        {
            switch (tipo)
            {
                case "lista-conteudos":
                {
                    var areaid = Str(config, "areaid");
                    if (string.IsNullOrEmpty(areaid)) return null;
                    return _context.Conteudos
                        .Where(c => c.Areaid == areaid)
                        .OrderByDescending(c => c.Datainclusao)
                        .Take(Int(config, "limite", 6))
                        .Select(c => new { c.Conteudoid, c.Titulo, c.Texto, c.Autor, c.Datainclusao })
                        .ToList();
                }
                case "lista-produtos":
                {
                    var catid = Str(config, "cateriaid");
                    var q = _context.Produtos.Where(p => p.Aplicacaoid == aplicacaoid);
                    if (!string.IsNullOrEmpty(catid)) q = q.Where(p => p.Cateriaid == catid);
                    return q.Take(Int(config, "limite", 8))
                        .Select(p => new {
                            p.Produtoid, p.Nome, p.Descricacurta, p.Valor,
                            Imagem = _context.Imagems.Where(i => i.Parentid == p.Produtoid).Select(i => i.Url).FirstOrDefault()
                        }).ToList();
                }
                case "categorias":
                {
                    var pai = Str(config, "cateriaidpai");
                    return _context.Cateria
                        .Where(c => c.Aplicacaoid == aplicacaoid &&
                            (string.IsNullOrEmpty(pai) ? c.Cateriaidpai == null : c.Cateriaidpai == pai))
                        .Select(c => new { c.Cateriaid, c.Nome, c.Descricao })
                        .ToList();
                }
                case "faq":
                {
                    var formularioid = Str(config, "formularioid");
                    if (string.IsNullOrEmpty(formularioid)) return null;
                    return _context.Faqs
                        .Where(f => f.Formularioid == formularioid && f.Ativo)
                        .OrderBy(f => f.Ordem)
                        .Select(f => new { f.Faqid, f.Pergunta, f.Resposta })
                        .ToList();
                }
                case "formulario":
                {
                    var formularioid = Str(config, "formularioid");
                    if (string.IsNullOrEmpty(formularioid)) return null;
                    var f = _context.Formularios.FirstOrDefault(x => x.Formularioid == formularioid);
                    return f == null ? null : new { f.Formularioid, f.Nome, f.Valor };
                }
                case "menu-navegacao":
                {
                    return _context.Areas
                        .Where(a => a.Aplicacaoid == aplicacaoid && !string.IsNullOrEmpty(a.Url))
                        .OrderBy(a => a.Posicao).ThenBy(a => a.Nome)
                        .Select(a => new { a.Areaid, a.Nome, a.Url })
                        .ToList();
                }
                case "prova-social":
                case "video":
                case "contador":
                case "hero-cta":
                case "rodape":
                    return null; // config-only blocks, no DB enrichment needed
                default: return null;
            }
        }

        private static string? Str(JsonElement el, string key) =>
            el.TryGetProperty(key, out var v) ? v.GetString() : null;

        private static int Int(JsonElement el, string key, int def) =>
            el.TryGetProperty(key, out var v) && v.TryGetInt32(out var n) ? n : def;
    }
}
