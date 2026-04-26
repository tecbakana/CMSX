using CMSXData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CMSAPIPublica.Controllers;

[ApiController]
[Route("api/publico/orcamento")]
[EnableRateLimiting("api_publica")]
public class OrcamentoPublicoController : ControllerBase
{
    private const int MaxDescricaoLen = 2000;

    private readonly CmsxDbContext _context;

    public OrcamentoPublicoController(CmsxDbContext context)
    {
        _context = context;
    }

    private string? ResolverAplicacaoid(string token)
    {
        var registro = _context.PublicTokens.FirstOrDefault(t =>
            t.Token == token &&
            t.Ativo &&
            (t.Datavencimento == null || t.Datavencimento > DateTime.UtcNow));

        return registro?.Aplicacaoid;
    }

    [HttpGet("produtos")]
    public IActionResult GetProdutos([FromQuery] string @ref)
    {
        if (string.IsNullOrWhiteSpace(@ref)) return BadRequest("token obrigatório");

        var aplicacaoid = ResolverAplicacaoid(@ref);
        if (aplicacaoid == null) return NotFound("token inválido ou expirado");

        var produtos = _context.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .OrderBy(p => p.Nome)
            .Select(p => new { p.Produtoid, p.Nome, p.Valor, p.UnidadeVenda })
            .ToList();

        var produtoIds = produtos.Select(p => p.Produtoid).ToList();

        var atributos = _context.Atributos
            .Where(a => a.Produtoid != null && produtoIds.Contains(a.Produtoid))
            .ToList();

        var atributoIds = atributos.Select(a => a.Atributoid).ToList();

        var opcoes = _context.Opcaos
            .Where(o => atributoIds.Contains(o.Atributoid))
            .Select(o => new { o.Opcaoid, o.Nome, o.Atributoid })
            .ToList();

        var result = produtos.Select(p => new
        {
            produtoid    = p.Produtoid,
            nome         = p.Nome,
            valor        = p.Valor,
            unidadeVenda = p.UnidadeVenda,
            atributos    = atributos
                .Where(a => a.Produtoid == p.Produtoid)
                .Select(a => new
                {
                    atributoid = a.Atributoid.ToString(),
                    nome       = a.Nome,
                    opcoes     = opcoes
                        .Where(o => o.Atributoid == a.Atributoid)
                        .Select(o => new { opcaoid = o.Opcaoid, nome = o.Nome })
                        .ToList()
                })
                .ToList()
        }).ToList();

        return Ok(result);
    }

    public class ItemOrcamentoDto
    {
        public string Descricao { get; set; } = "";
        public decimal Quantidade { get; set; } = 1;
        public decimal? Valor { get; set; }
        public bool Ativo { get; set; } = true;
    }

    public class NovoOrcamentoPublicoDto
    {
        public string Token { get; set; } = "";
        public string Nome { get; set; } = "";
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Descricaoservico { get; set; }
        public decimal? Valorestimado { get; set; }
        public string? Prazo { get; set; }
        public string? Nomevendedor { get; set; }
        public List<ItemOrcamentoDto> Itens { get; set; } = new();
    }

    [HttpPost]
    public IActionResult Criar([FromBody] NovoOrcamentoPublicoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token))
            return BadRequest("token obrigatório");

        if (string.IsNullOrWhiteSpace(dto.Nome))
            return BadRequest("nome obrigatório");

        var aplicacaoid = ResolverAplicacaoid(dto.Token);
        if (aplicacaoid == null) return NotFound("token inválido ou expirado");

        var descricao = dto.Descricaoservico?.Trim();
        if (descricao?.Length > MaxDescricaoLen)
            descricao = descricao[..MaxDescricaoLen];

        var orcamento = new OrcamentoCabecalho
        {
            Orcamentoid      = Guid.NewGuid(),
            Aplicacaoid      = aplicacaoid,
            Nome             = dto.Nome.Trim(),
            Email            = dto.Email?.Trim(),
            Telefone         = dto.Telefone?.Trim(),
            Descricaoservico = descricao,
            Valorestimado    = dto.Valorestimado,
            Prazo            = dto.Prazo?.Trim(),
            Nomevendedor     = dto.Nomevendedor?.Trim(),
            Aprovado         = false,
            Datainclusao     = DateTime.UtcNow
        };

        _context.OrcamentoCabecalhos.Add(orcamento);

        foreach (var item in dto.Itens.Where(i => !string.IsNullOrWhiteSpace(i.Descricao)))
        {
            _context.OrcamentoDetalhes.Add(new OrcamentoDetalhe
            {
                Orcamentodetalheid = Guid.NewGuid(),
                Orcamentoid        = orcamento.Orcamentoid,
                Descricao          = item.Descricao.Trim(),
                Quantidade         = item.Quantidade,
                Valor              = item.Valor,
                Ativo              = item.Ativo
            });
        }

        _context.SaveChanges();
        return Ok(new { orcamento.Orcamentoid });
    }
}
