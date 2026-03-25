namespace CMSUI.Models;

public partial class Aplicacao
{
    public string Aplicacaoid { get; set; } = null!;
    public string? Nome { get; set; }
    public string? Url { get; set; }
    public DateTime? Datainicio { get; set; }
    public DateTime? Datafinal { get; set; }
    public string? Idusuarioinicio { get; set; }
    public string? Idusuariofim { get; set; }
    public string? Pagsegurotoken { get; set; }
    public string? Layoutchoose { get; set; }
    public int? Posicao { get; set; }
    public string? Mailuser { get; set; }
    public string? Mailpassword { get; set; }
    public string? Mailserver { get; set; }
    public int? Mailport { get; set; }
    public byte? Issecure { get; set; }
    public string? Pagefacebook { get; set; }
    public string? Pagelinkedin { get; set; }
    public string? Pageinstagram { get; set; }
    public string? Pagetwitter { get; set; }
    public string? Pagepinterest { get; set; }
    public string? Pageflicker { get; set; }
    public byte[]? Lotipo { get; set; }
    public string? Ogleadsense { get; set; }
    public string? Header { get; set; }
    public bool? Isactive { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Descricao { get; set; }
    public bool IsDemo { get; set; }
}
