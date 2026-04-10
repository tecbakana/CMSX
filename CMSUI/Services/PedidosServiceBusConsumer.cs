using Azure.Messaging.ServiceBus;
using CMSUI.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CMSUI.Services;

// Mensagem esperada no tópico top-status-pedidos / sub-status-cmsx:
// {
//   "aplicacaoid":  "<guid-do-tenant>",
//   "numeropedido": "<codigo-externo>",
//   "clientenome":  "<nome>",
//   "clienteemail": "<email>",
//   "valorpedido":  12.50,
//   "status":       "entrada|confirmado|pagamento",
//   "descricao":    "<mensagem opcional>"
// }

public class PedidosServiceBusConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<PedidosServiceBusConsumer> _logger;

    public PedidosServiceBusConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<PedidosServiceBusConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connStr  = _config["ServiceBus:ConnectionString"];
        var topico   = _config["ServiceBus:Topico"]       ?? "top-status-pedidos";
        var sub      = _config["ServiceBus:Subscription"] ?? "sub-status-cmsx";

        if (string.IsNullOrWhiteSpace(connStr))
        {
            _logger.LogWarning("ServiceBus:ConnectionString não configurada — consumer desabilitado.");
            return;
        }

        await using var client    = new ServiceBusClient(connStr);
        await using var processor = client.CreateProcessor(topico, sub, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false
        });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync   += HandleErrorAsync;

        await processor.StartProcessingAsync(stoppingToken);
        _logger.LogInformation("Consumer Service Bus iniciado. Tópico={Topico} Sub={Sub}", topico, sub);

        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) { }

        await processor.StopProcessingAsync();
        _logger.LogInformation("Consumer Service Bus encerrado.");
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var json = args.Message.Body.ToString();
            var msg  = JsonSerializer.Deserialize<PedidoStatusMsg>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (msg is null || string.IsNullOrWhiteSpace(msg.Numeropedido) || string.IsNullOrWhiteSpace(msg.Aplicacaoid))
            {
                _logger.LogWarning("Mensagem inválida recebida: {Body}", json);
                await args.CompleteMessageAsync(args.Message);
                return;
            }

            using var scope   = _scopeFactory.CreateScope();
            var ctx           = scope.ServiceProvider.GetRequiredService<CmsxDbContext>();

            var pedido = await ctx.Pedidos
                .FirstOrDefaultAsync(p => p.Numeropedido == msg.Numeropedido && p.Aplicacaoid == msg.Aplicacaoid);

            if (pedido is null)
            {
                pedido = new Pedido
                {
                    Pedidoid     = Guid.NewGuid(),
                    Aplicacaoid  = msg.Aplicacaoid,
                    Numeropedido = msg.Numeropedido,
                    Clientenome  = msg.Clientenome,
                    Clienteemail = msg.Clienteemail,
                    Valorpedido  = msg.Valorpedido,
                    Statusatual  = msg.Status,
                    Datainclusao = DateTime.UtcNow
                };
                ctx.Pedidos.Add(pedido);
            }
            else
            {
                pedido.Statusatual = msg.Status;
                if (!string.IsNullOrWhiteSpace(msg.Clientenome))  pedido.Clientenome  = msg.Clientenome;
                if (!string.IsNullOrWhiteSpace(msg.Clienteemail)) pedido.Clienteemail = msg.Clienteemail;
                if (msg.Valorpedido.HasValue)                      pedido.Valorpedido  = msg.Valorpedido;
            }

            ctx.Statuspedidos.Add(new Statuspedido
            {
                Statuspedidoid = Guid.NewGuid(),
                Pedidoid       = pedido.Pedidoid,
                Status         = msg.Status,
                Descricao      = msg.Descricao,
                Datahora       = DateTime.UtcNow
            });

            pedido.Statusatual = msg.Status;

            switch (msg.Evento)
            {
                case "pagamento.confirmado":
                    // baixar estoque aqui (quando implementado)
                    break;
                case "pagamento.recusado":
                case "pagamento.timeout":
                case "pagamento.erro":
                    // notificar operador aqui (quando implementado)
                    break;
                case "pagamento.pendente":
                    // guardar link/codigo aqui (quando campos existirem no modelo)
                    break;
            }

            await ctx.SaveChangesAsync();
            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation("Pedido {Num} — status '{Status}' registrado.", msg.Numeropedido, msg.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem do Service Bus.");
            await args.AbandonMessageAsync(args.Message);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Erro no processor Service Bus. Origem={Source}", args.ErrorSource);
        return Task.CompletedTask;
    }

    private sealed class PedidoStatusMsg
    {
        public string?  Aplicacaoid  { get; set; }
        public string?  Numeropedido { get; set; }
        public string?  Clientenome  { get; set; }
        public string?  Clienteemail { get; set; }
        public decimal? Valorpedido  { get; set; }
        public string?  Status       { get; set; }
        public string?  Descricao    { get; set; }
        public string? Evento { get; set; }
    }
}
