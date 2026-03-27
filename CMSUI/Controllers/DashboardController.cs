using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSUI.Models;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : Controller
    {
        private readonly CmsxDbContext _context;
        public DashboardController(CmsxDbContext context) { _context = context; }

        [HttpGet]
        [Authorize]
        public IActionResult Get([FromQuery] string? aplicacaoid = null)
        {
            var acessoTotal = User.FindFirstValue("acessoTotal") == "True";
            var claimAppId  = User.FindFirstValue("aplicacaoid");

            // Admin sem tenant selecionado: totais globais
            if (acessoTotal && string.IsNullOrEmpty(aplicacaoid))
            {
                return Ok(new
                {
                    usuarios   = _context.Usuarios.Count(),
                    aplicacoes = _context.Aplicacaos.Count(),
                    conteudos  = _context.Conteudos.Count(),
                    areas      = _context.Areas.Count(),
                    categorias = _context.Cateria.Count(),
                    modulos    = _context.Modulos.Count()
                });
            }

            var filtroId = acessoTotal ? aplicacaoid : claimAppId;

            var areasIds = _context.Areas
                .Where(a => a.Aplicacaoid == filtroId)
                .Select(a => a.Areaid)
                .ToHashSet();

            return Ok(new
            {
                usuarios   = _context.Relusuarioaplicacaos.Count(r => r.Aplicacaoid == filtroId),
                aplicacoes = _context.Aplicacaos.Count(a => a.Aplicacaoid == filtroId),
                conteudos  = _context.Conteudos.Count(c => c.Areaid != null && areasIds.Contains(c.Areaid)),
                areas      = areasIds.Count,
                categorias = _context.Cateria.Count(c => c.Aplicacaoid == filtroId),
                modulos    = _context.Relmoduloaplicacaos.Count(r => r.Aplicacaoid == filtroId)
            });
        }
    }
}
