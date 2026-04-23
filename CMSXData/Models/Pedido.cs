using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class Pedido
{
    public Guid Pedidoid { get; set; }
    public string? Aplicacaoid { get; set; }
    public string? Numeropedido { get; set; }
    public string? Clientenome { get; set; }
    public string? Clienteemail { get; set; }
    public decimal? Valorpedido { get; set; }
    public string? Statusatual { get; set; }
    public string? MetodoPagamento { get; set; }
    public DateTime? Datainclusao { get; set; }

    public virtual ICollection<Statuspedido> Statuspedidos { get; set; } = new List<Statuspedido>();
}
