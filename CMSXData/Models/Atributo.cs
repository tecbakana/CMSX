using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class Atributo
{
    public Guid Atributoid { get; set; }

    public string Nome { get; set; } = null!;

    public string Descricao { get; set; } = null!;

    public string? Produtoid { get; set; }

    public Guid? ParentAtributoId { get; set; }

    public int? Ordem { get; set; }

    public decimal? ValorAdicional { get; set; }

    public virtual Atributo? Parent { get; set; }

    public virtual ICollection<Atributo> Filhos { get; set; } = new List<Atributo>();

    public virtual ICollection<Opcao> Opcoes { get; set; } = new List<Opcao>();
}
