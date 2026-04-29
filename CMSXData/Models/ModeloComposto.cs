using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class ModeloComposto
{
    public Guid ModeloCompostoId { get; set; }

    public string Aplicacaoid { get; set; } = null!;

    public string Produtoid { get; set; } = null!;

    public string Nome { get; set; } = null!;

    public decimal ValorUnitario { get; set; }

    public string ConfiguracaoHash { get; set; } = null!;

    public int Usos { get; set; }

    public DateTime Datacriacao { get; set; }

    public virtual ICollection<ModeloSelecao> Selecoes { get; set; } = new List<ModeloSelecao>();
}
