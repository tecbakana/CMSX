using System;

namespace CMSXData.Models;

public partial class ClienteLoja
{
    public string ClienteLojaid { get; set; } = string.Empty;
    public string Aplicacaoid { get; set; } = string.Empty;
    public int SalematicClienteId { get; set; }
    public DateTime? Datainclusao { get; set; }
}
