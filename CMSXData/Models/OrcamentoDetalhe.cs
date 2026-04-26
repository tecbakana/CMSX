using System;

namespace CMSXData.Models;

public partial class OrcamentoDetalhe
{
    public Guid Orcamentodetalheid { get; set; }
    public Guid Orcamentoid { get; set; }
    public string Descricao { get; set; } = null!;
    public decimal Quantidade { get; set; }
    public decimal? Valor { get; set; }
    public bool Ativo { get; set; }

    public virtual OrcamentoCabecalho OrcamentoCabecalhoNavigation { get; set; } = null!;
}
