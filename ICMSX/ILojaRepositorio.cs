using CMSXData.Models;

namespace ICMSX;

public interface ILojaRepositorio
{
    Aplicacao? ResolveAplicacao(string slug);
    string? ResolvePublicToken(string token);
    string? GetActiveTokenForApp(string aplicacaoid);
    Task<IEnumerable<Produto>> ListaCatalogoAsync(string aplicacaoid);
    Task<Pedido> CriaPedidoAsync(Pedido pedido);
    Task AtualizaStatusPedidoAsync(Pedido pedido, string status, string descricao);
    Task<Pedido?> BuscaPedidoComTimelineAsync(Guid pedidoId);
    IEnumerable<Pedido> ListaPedidosPorCliente(string clienteEmail);
}
