using System;

namespace CMSXData.Models;

public partial class PublicToken
{
    public Guid PublicTokenId { get; set; }

    public string Token { get; set; } = null!;

    public string Aplicacaoid { get; set; } = null!;

    public bool Ativo { get; set; }

    public DateTime Datainclusao { get; set; }

    public DateTime? Datavencimento { get; set; }
}
