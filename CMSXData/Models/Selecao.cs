using System;

namespace CMSXData.Models;

public partial class Selecao
{
    public Guid SelecaoId { get; set; }

    public Guid OrcamentoDetalheCompostoId { get; set; }

    public Guid Atributoid { get; set; }

    public string Opcaoid { get; set; } = null!;

    public decimal ValorAdicional { get; set; }

    public virtual OrcamentoDetalheComposto Detalhe { get; set; } = null!;

    public virtual Atributo Atributo { get; set; } = null!;

    public virtual Opcao Opcao { get; set; } = null!;
}
