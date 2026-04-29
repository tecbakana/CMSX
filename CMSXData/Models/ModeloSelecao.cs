using System;

namespace CMSXData.Models;

public partial class ModeloSelecao
{
    public Guid ModeloSelecaoId { get; set; }

    public Guid ModeloCompostoId { get; set; }

    public Guid Atributoid { get; set; }

    public string Opcaoid { get; set; } = null!;

    public virtual ModeloComposto Modelo { get; set; } = null!;
}
