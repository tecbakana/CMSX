using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMSAPI.Services;
using CMSXData.Models;
using ICMSX;
using System.Security.Claims;

namespace CMSAPI.Controllers;

[ApiController]
[Route("api/loja")]
[AllowAnonymous]
public class LojaController(
    SalematicHttpService salematic,
    CmsxDbContext db,
    IClienteLojaRepositorio clienteLojaRepo,
    PedidoServiceBusPublisher publisher) : ControllerBase
{

    [HttpGet("resolve")]
    public IActionResult Resolve([FromQuery] string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return BadRequest(new { message = "slug é obrigatório." });

        var app = db.Aplicacaos.FirstOrDefault(a => a.Url == slug);
        if (app == null)
            return NotFound(new { message = $"Site '{slug}' não encontrado." });

        return Ok(new { aplicacaoid = app.Aplicacaoid.ToString(), nomeLoja = app.Nome });
    }

    [HttpGet("catalogo")]
    public async Task<IActionResult> GetProdutos([FromQuery] string aplicacaoid)
    {
        if (string.IsNullOrEmpty(aplicacaoid))
            return BadRequest(new { message = "aplicacaoid é obrigatório." });

        var produtos = await db.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .Select(p => new {
                p.Produtoid,
                p.Sku,
                p.Nome,
                p.Descricacurta,
                p.Valor
            })
            .ToListAsync();

        return Ok(produtos);
    }

    [HttpPost("auth/registrar")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Aplicacaoid))
            return BadRequest(new { message = "aplicacaoid é obrigatório." });

        var auth = await salematic.RegistrarAsync(req);
        if (auth is null)
            return StatusCode(502, new { message = "Falha ao registrar cliente na Salematic." });

        dynamic connProps = new System.Dynamic.ExpandoObject();
        connProps.banco = "SqlServer";
        connProps.parms = 3;
        clienteLojaRepo.MakeConnection(connProps);
        clienteLojaRepo.CriaClienteLoja(new ClienteLoja
        {
            Aplicacaoid        = req.Aplicacaoid,
            SalematicClienteId = auth.ClienteId
        });

        return Ok(auth);
    }

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] LoginLojaRequest req)
    {
        var auth = await salematic.LoginAsync(req);
        if (auth is null)
            return Unauthorized(new { message = "Credenciais inválidas." });

        return Ok(auth);
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpPost("pedidos")]
    public async Task<IActionResult> CriarPedido([FromBody] CriarPedidoLojaRequest req)
    {
        if (string.IsNullOrEmpty(req.Numeropedido) || string.IsNullOrEmpty(req.Clienteemail))
            return BadRequest(new { message = "numeropedido e clienteemail são obrigatórios." });

        var pedido = new Pedido
        {
            Pedidoid        = Guid.NewGuid(),
            Aplicacaoid     = req.Aplicacaoid,
            Numeropedido    = req.Numeropedido,
            Clientenome     = req.Clientenome,
            Clienteemail    = req.Clienteemail,
            Valorpedido     = req.Valorpedido,
            MetodoPagamento = req.MetodoPagamento,
            Statusatual     = "pendente",
            Datainclusao    = DateTime.UtcNow
        };

        db.Pedidos.Add(pedido);
        db.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid       = pedido.Pedidoid,
            Status         = "pendente",
            Descricao      = "Pedido recebido.",
            Datahora       = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        try
        {
            await publisher.PublicarPedidoAsync(pedido);

            pedido.Statusatual = "criado";
            db.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "criado",
                Descricao      = "Pedido publicado no Service Bus com sucesso.",
                Datahora       = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        catch (Exception)
        {
            db.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "erro_envio",
                Descricao      = "Falha ao publicar no Service Bus. Pedido pendente de reenvio.",
                Datahora       = DateTime.UtcNow
            });
            pedido.Statusatual = "erro_envio";
            await db.SaveChangesAsync();
        }

        return Created($"/api/loja/pedidos/{pedido.Pedidoid}/timeline", new
        {
            pedido.Pedidoid,
            pedido.Numeropedido,
            pedido.Statusatual
        });
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpGet("pedidos/{id:guid}/timeline")]
    public async Task<IActionResult> GetTimeline(Guid id)
    {
        var clienteEmail = User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;

        var pedido = await db.Pedidos
            .Include(p => p.Statuspedidos)
            .FirstOrDefaultAsync(p => p.Pedidoid == id);

        if (pedido is null)
            return NotFound(new { message = "Pedido não encontrado." });

        if (pedido.Clienteemail != clienteEmail)
            return Forbid();

        var timeline = pedido.Statuspedidos
            .OrderBy(s => s.Datahora)
            .Select(s => new { s.Status, s.Descricao, s.Datahora });

        return Ok(new
        {
            pedido.Pedidoid,
            pedido.Numeropedido,
            pedido.Statusatual,
            timeline
        });
    }

    [Authorize(AuthenticationSchemes = "Salematic")]
    [HttpGet("meus-pedidos")]
    public IActionResult MeusPedidos()
    {
        var clienteEmail = User.FindFirst("email")?.Value
                    ?? User.FindFirst(ClaimTypes.Email)?.Value;

        if(string.IsNullOrEmpty(clienteEmail))
            return Unauthorized(new { message = "Email nao encontrado" });

        var pedidos = db.Pedidos
            .Where(p => p.Clienteemail == clienteEmail)
            .OrderByDescending(p => p.Datainclusao)
            .Select(p => new {
                p.Pedidoid,
                p.Aplicacaoid,
                p.Numeropedido,
                p.Clientenome,
                p.Clienteemail,
                p.Valorpedido,
                p.Statusatual,
                p.Datainclusao
            })
            .ToArray();

        return Ok(pedidos);
    }
}
