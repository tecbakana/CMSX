using CMSXData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CMSAPIPublica.Controllers;

[ApiController]
[Route("api/publico/token")]
[EnableRateLimiting("api_publica")]
public class PublicTokenController : ControllerBase
{
    private readonly CmsxDbContext _context;

    public PublicTokenController(CmsxDbContext context)
    {
        _context = context;
    }

    [HttpGet("resolve/{token}")]
    public IActionResult Resolve(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return BadRequest();

        var registro = _context.PublicTokens.FirstOrDefault(t =>
            t.Token == token &&
            t.Ativo &&
            (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow));

        if (registro == null) return NotFound();

        return Ok(new { registro.Aplicacaoid });
    }
}
