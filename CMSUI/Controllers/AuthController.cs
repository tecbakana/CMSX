using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CMSUI.Models;

namespace CMSUI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly CmsxDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(CmsxDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public class LoginRequest { public string Apelido { get; set; } = ""; public string Senha { get; set; } = ""; }

        public class SignupRequest
        {
            public string Nome { get; set; } = "";
            public string Sobrenome { get; set; } = "";
            public string Apelido { get; set; } = "";
            public string Senha { get; set; } = "";
            public string AppNome { get; set; } = "";
            public string AppUrl { get; set; } = "";
        }

        [HttpPost("signup")]
        public IActionResult Signup([FromBody] SignupRequest req)
        {
            if (_context.Usuarios.Any(u => u.Apelido == req.Apelido))
                return BadRequest(new { message = "Login já está em uso." });

            if (_context.Aplicacaos.Any(a => a.Url == req.AppUrl))
                return BadRequest(new { message = "URL da aplicação já está em uso." });

            var userId = Guid.NewGuid().ToString();
            var appId  = Guid.NewGuid().ToString();

            var usuario = new Usuario
            {
                Userid       = userId,
                Nome         = req.Nome,
                Sobrenome    = req.Sobrenome,
                Apelido      = req.Apelido,
                Senha        = req.Senha,
                Ativo        = 0,
                Datainclusao = DateTime.Now
            };

            var aplicacao = new Aplicacao
            {
                Aplicacaoid     = appId,
                Nome            = req.AppNome,
                Url             = req.AppUrl,
                Idusuarioinicio = userId,
                Datainicio      = DateTime.Now,
                Isactive        = false,
                Layoutchoose    = "_Layout.cshtml"
            };

            var relacao = new Relusuarioaplicacao
            {
                Usuarioid   = userId,
                Aplicacaoid = appId,
                Relacaoid   = Guid.NewGuid().ToString()
            };

            _context.Usuarios.Add(usuario);
            _context.Aplicacaos.Add(aplicacao);
            _context.Relusuarioaplicacaos.Add(relacao);
            _context.SaveChanges();

            return Ok(new { message = "Cadastro realizado com sucesso." });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var user = _context.Usuarios
                .FirstOrDefault(u => u.Apelido == req.Apelido && u.Senha == req.Senha && u.Ativo == (byte)1);
            if (user == null)
                return Unauthorized(new { message = "Login ou senha inválidos" });

            var grupos = _context.Relusuariogrupos
                .Where(r => r.Usuarioid == user.Userid)
                .Join(_context.Grupos, r => r.Grupoid, g => g.Grupoid, (r, g) => g)
                .ToList();

            bool acessoTotal = grupos.Any(g => g.Acessototal);
            var nomesGrupos  = grupos.Select(g => g.Nome).ToList();

            var apps = _context.Relusuarioaplicacaos
                .Where(r => r.Usuarioid == user.Userid)
                .Select(r => r.Aplicacaoid)
                .ToList();

            string? aplicacaoid = apps.FirstOrDefault();

            var app = _context.Aplicacaos.FirstOrDefault(a => a.Aplicacaoid == aplicacaoid);
            bool isDemo = app?.IsDemo ?? false;

            var claims = new[]
            {
                new Claim("userid",      user.Userid),
                new Claim("apelido",     user.Apelido ?? ""),
                new Claim("nome",        user.Nome ?? ""),
                new Claim("aplicacaoid", aplicacaoid ?? ""),
                new Claim("acessoTotal", acessoTotal.ToString()),
                new Claim("isDemo",      isDemo.ToString())
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:            _config["Jwt:Issuer"],
                audience:          _config["Jwt:Audience"],
                claims:            claims,
                expires:           DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return Ok(new
            {
                token       = new JwtSecurityTokenHandler().WriteToken(token),
                userid      = user.Userid,
                nome        = user.Nome,
                apelido     = user.Apelido,
                acessoTotal,
                grupos      = nomesGrupos,
                aplicacaoid,
                isDemo
            });
        }

        // ── Login demo: reseta dados e emite token sem credenciais ───────────
        [HttpPost("demo-login")]
        public IActionResult DemoLogin()
        {
            var user = _context.Usuarios.FirstOrDefault(u => u.Apelido == "demo" && u.Ativo == (byte)1);
            if (user == null)
                return NotFound(new { message = "Tenant demo não configurado. Execute cmsxDB.tenant_demo.sql." });

            var aplicacaoid = _context.Relusuarioaplicacaos
                .Where(r => r.Usuarioid == user.Userid)
                .Select(r => r.Aplicacaoid)
                .FirstOrDefault();

            if (aplicacaoid == null)
                return StatusCode(500, new { message = "Tenant demo sem aplicação vinculada." });

            // Reset: limpa layouts de todas as áreas do tenant demo
            var areas = _context.Areas.Where(a => a.Aplicacaoid == aplicacaoid).ToList();
            foreach (var area in areas)
                area.Layout = "{\"blocos\":[]}";

            // Remove conteúdos do tenant demo
            var areaIds = areas.Select(a => a.Areaid).ToList();
            var conteudos = _context.Conteudos.Where(c => areaIds.Contains(c.Areaid)).ToList();
            _context.Conteudos.RemoveRange(conteudos);

            // Zera uso de IA do dia
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var usoHoje = _context.IaUsos.Where(u => u.Aplicacaoid == aplicacaoid && u.Data == hoje).ToList();
            _context.IaUsos.RemoveRange(usoHoje);

            _context.SaveChanges();

            var claims = new[]
            {
                new Claim("userid",      user.Userid),
                new Claim("apelido",     user.Apelido ?? ""),
                new Claim("nome",        "Demo"),
                new Claim("aplicacaoid", aplicacaoid),
                new Claim("acessoTotal", "False"),
                new Claim("isDemo",      "True")
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:             _config["Jwt:Issuer"],
                audience:           _config["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token       = new JwtSecurityTokenHandler().WriteToken(token),
                userid      = user.Userid,
                nome        = "Demo",
                apelido     = "demo",
                acessoTotal = false,
                grupos      = new string[0],
                aplicacaoid,
                isDemo      = true
            });
        }
    }
}
