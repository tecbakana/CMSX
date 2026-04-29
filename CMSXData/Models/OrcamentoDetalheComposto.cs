using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class OrcamentoDetalheComposto
{
    public Guid OrcamentoDetalheCompostoId { get; set; }

    public Guid Orcamentoid { get; set; }

    public string Produtoid { get; set; } = null!;

    public decimal Quantidade { get; set; }

    public decimal ValorBase { get; set; }

    public decimal ValorTotal { get; set; }

    public string ConfiguracaoJson { get; set; } = null!;

    public int Versao { get; set; }

    public bool Atual { get; set; }

    public DateTime Datainclusao { get; set; }

    public virtual OrcamentoCabecalho Cabecalho { get; set; } = null!;

    public virtual Produto Produto { get; set; } = null!;

    public virtual ICollection<Selecao> Selecoes { get; set; } = new List<Selecao>();
}
