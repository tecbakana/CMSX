using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class OrcamentoCabecalho
{
    public Guid Orcamentoid { get; set; }
    public string Aplicacaoid { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Descricaoservico { get; set; }
    public decimal? Valorestimado { get; set; }
    public string? Prazo { get; set; }
    public string? Nomevendedor { get; set; }
    public bool Aprovado { get; set; }
    public DateTime? Datainclusao { get; set; }

    public virtual ICollection<OrcamentoDetalhe> OrcamentoDetalhes { get; set; } = new List<OrcamentoDetalhe>();
}
