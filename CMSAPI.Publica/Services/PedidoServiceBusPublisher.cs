using Azure.Messaging.ServiceBus;
using CMSXData.Models;
using ICMSX;
using System.Text.Json;

namespace CMSAPIPublica.Services;

public class PedidoServiceBusPublisher : IEventPublisher
{
    private readonly ILogger<PedidoServiceBusPublisher> _logger;
    private readonly ServiceBusSender? _sender;

    public PedidoServiceBusPublisher(IConfiguration config, ILogger<PedidoServiceBusPublisher> logger)
    {
        _logger = logger;
        var connStr = config["ServiceBus:ConnectionString"];
        var topico = config["ServiceBus:TopicoPedidos"] ?? "top-pedidos";
        if (string.IsNullOrWhiteSpace(connStr))
        {
            _logger.LogWarning("ServiceBus:ConnectionString não configurada — publisher desabilitado.");
            return;
        }
        var client = new ServiceBusClient(connStr);
        _sender = client.CreateSender(topico);
    }

    public async Task PublicarPedidoAsync(Pedido pedido)
    {
        if (_sender == null)
        {
            _logger.LogWarning("Publisher de pedidos não configurado — evento não publicado.");
            return;
        }
        var mensagem = new
        {
            aplicacaoid  = pedido.Aplicacaoid,
            numeropedido = pedido.Numeropedido,
            clientenome  = pedido.Clientenome,
            clienteemail = pedido.Clienteemail,
            valorpedido  = pedido.Valorpedido,
            status       = pedido.Statusatual!.ToLower(),
            descricao    = $"Pedido {pedido.Statusatual!.ToLower()} no CMSX"
        };
        var message = new ServiceBusMessage(JsonSerializer.Serialize(mensagem));
        try
        {
            await _sender.SendMessageAsync(message);
            _logger.LogInformation("Evento de pedido publicado: {Numeropedido} Status={Status}", pedido.Numeropedido, pedido.Statusatual);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento de pedido: {Numeropedido}", pedido.Numeropedido);
        }
    }
}
