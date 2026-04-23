using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CMSXData.Models;
using CMSAPI.Services;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class PedidosController : Controller
{
    private readonly CmsxDbContext _context;
    private readonly PedidoServiceBusPublisher _publisher;

    public PedidosController(CmsxDbContext context, PedidoServiceBusPublisher publisher)
    {
        _context   = context;
        _publisher = publisher;
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True", User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public IActionResult Get([FromQuery] string? aplicacaoid = null, [FromQuery] string? status = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var q = _context.Pedidos.AsQueryable();

        if (acessoTotal)
        {
            if (!string.IsNullOrEmpty(aplicacaoid))
                q = q.Where(p => p.Aplicacaoid == aplicacaoid);
        }
        else
        {
            q = q.Where(p => p.Aplicacaoid == claimAppId);
        }

        if (!string.IsNullOrEmpty(status))
            q = q.Where(p => p.Statusatual == status);

        var resultado = q
            .OrderByDescending(p => p.Datainclusao)
            .Select(p => new
            {
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

        return Ok(resultado);
    }

    [HttpGet("{id}/timeline")]
    public IActionResult GetTimeline(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Pedidoid == id);
        if (pedido is null) return NotFound();
        if (!acessoTotal && pedido.Aplicacaoid != claimAppId) return Forbid();

        var timeline = _context.Statuspedidos
            .Where(s => s.Pedidoid == id)
            .OrderBy(s => s.Datahora)
            .Select(s => new
            {
                s.Statuspedidoid,
                s.Status,
                s.Descricao,
                s.Datahora
            })
            .ToArray();

        return Ok(timeline);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] NovoPedido dto)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = dto.Aplicacaoid ?? claimAppId;
        if (!acessoTotal && appId != claimAppId) return Forbid();

        var pedido = new Pedido
        {
            Pedidoid     = Guid.NewGuid(),
            Aplicacaoid  = appId,
            Numeropedido = dto.Numeropedido,
            Clientenome  = dto.Clientenome,
            Clienteemail = dto.Clienteemail,
            Valorpedido  = dto.Valorpedido,
            MetodoPagamento = dto.MetodoPagamento,
            Statusatual  = "aguardando_envio",
            Datainclusao = DateTime.UtcNow
        };

        _context.Pedidos.Add(pedido);
        _context.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid       = pedido.Pedidoid,
            Status         = "aguardando_envio",
            Descricao      = "Pedido recebido, aguardando envio ao processador.",
            Datahora       = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        try
        {
            await _publisher.PublicarPedidoAsync(pedido);

            pedido.Statusatual = "criado";
            _context.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "criado",
                Descricao      = "Pedido publicado no Service Bus com sucesso.",
                Datahora       = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Compensação: mantém status aguardando_envio para reprocessamento posterior
            _context.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "erro_envio",
                Descricao      = "Falha ao publicar no Service Bus. Pedido pendente de reenvio.",
                Datahora       = DateTime.UtcNow
            });
            pedido.Statusatual = "erro_envio";
            await _context.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetTimeline), new { id = pedido.Pedidoid }, new
        {
            pedido.Pedidoid,
            pedido.Aplicacaoid,
            pedido.Numeropedido,
            pedido.Clientenome,
            pedido.Clienteemail,
            pedido.Valorpedido,
            pedido.Statusatual,
            pedido.Datainclusao
        });
    }

    [HttpPost("{id}/reenviar")]
    [AllowAnonymous]
    public async Task<IActionResult> Reenviar(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var pedido = _context.Pedidos.FirstOrDefault(p => p.Pedidoid == id);
        if (pedido is null) return NotFound();
        if (!acessoTotal && pedido.Aplicacaoid != claimAppId) return Forbid();
        if (pedido.Statusatual != "erro_envio" && pedido.Statusatual != "aguardando_envio")
            return BadRequest(new { message = "Pedido não está pendente de reenvio." });

        try
        {
            await _publisher.PublicarPedidoAsync(pedido);

            pedido.Statusatual = "criado";
            _context.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = "criado",
                Descricao      = "Pedido reenviado ao Service Bus com sucesso.",
                Datahora       = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return Ok(new { message = "Pedido reenviado com sucesso." });
        }
        catch (Exception)
        {
            return StatusCode(502, new { message = "Falha ao publicar no Service Bus. Tente novamente." });
        }
    }

    public class NovoPedido : Pedido
    {
       
    }
}
