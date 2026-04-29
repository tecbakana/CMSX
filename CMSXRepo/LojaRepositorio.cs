using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class LojaRepositorio : BaseRepositorio, ILojaRepositorio
{
    public LojaRepositorio(CmsxDbContext db) : base(db) { }

    public Aplicacao? ResolveAplicacao(string slug) =>
        _db.Aplicacaos.FirstOrDefault(a => a.Url == slug);

    public string? ResolvePublicToken(string token) =>
        _db.PublicTokens
            .Where(t => t.Token == token && t.Ativo &&
                   (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .Select(t => t.Aplicacaoid)
            .FirstOrDefault();

    public string? GetActiveTokenForApp(string aplicacaoid) =>
        _db.PublicTokens
            .Where(t => t.Aplicacaoid == aplicacaoid && t.Ativo &&
                   (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow))
            .OrderByDescending(t => t.Datainclusao)
            .Select(t => t.Token)
            .FirstOrDefault();

    public async Task<IEnumerable<Produto>> ListaCatalogoAsync(string aplicacaoid) =>
        await _db.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .ToListAsync();

    public async Task<Pedido> CriaPedidoAsync(Pedido pedido)
    {
        pedido.Pedidoid = Guid.NewGuid();
        pedido.Statusatual = "pendente";
        pedido.Datainclusao = DateTime.UtcNow;

        _db.Pedidos.Add(pedido);
        _db.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid = pedido.Pedidoid,
            Status = "pendente",
            Descricao = "Pedido recebido.",
            Datahora = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return pedido;
    }

    public async Task AtualizaStatusPedidoAsync(Pedido pedido, string status, string descricao)
    {
        pedido.Statusatual = status;
        _db.Statuspedidos.Add(new Statuspedido
        {
            Statuspedidoid = Guid.NewGuid(),
            Pedidoid = pedido.Pedidoid,
            Status = status,
            Descricao = descricao,
            Datahora = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<Pedido?> BuscaPedidoComTimelineAsync(Guid pedidoId) =>
        await _db.Pedidos
            .Include(p => p.Statuspedidos)
            .FirstOrDefaultAsync(p => p.Pedidoid == pedidoId);

    public IEnumerable<Pedido> ListaPedidosPorCliente(string clienteEmail) =>
        _db.Pedidos
            .Where(p => p.Clienteemail == clienteEmail)
            .OrderByDescending(p => p.Datainclusao)
            .ToArray();
}
