using System;
using System.Collections.Generic;

namespace CMSXData.Models;

public partial class Produto
{
    public string Produtoid { get; set; } = null!;

    public string? Nome { get; set; }

    public string? Descricao { get; set; }

    public decimal? Valor { get; set; }

    public int? Tipo { get; set; }

    public int? Destaque { get; set; }

    public DateTime? Datainicio { get; set; }

    public DateTime? Datafinal { get; set; }

    public string? Cateriaid { get; set; }

    public string? Aplicacaoid { get; set; }

    public string Sku { get; set; } = null!;

    public string? Pagsegurokey { get; set; }

    public string? Detalhetecnico { get; set; }

    public string? Descricacurta { get; set; }

    public string? Produtocol { get; set; }

    public int? SalematicProdutoid { get; set; }

    public string? UnidadeVenda { get; set; }
}
