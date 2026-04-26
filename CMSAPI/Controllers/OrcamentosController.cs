using ICMSX;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CMSAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class OrcamentosController : Controller
{
    private readonly IOrcamentoRepositorio _orcamentoRepo;

    public OrcamentosController(IOrcamentoRepositorio orcamentoRepo)
    {
        _orcamentoRepo = orcamentoRepo;
    }

    private (bool acessoTotal, string? aplicacaoid) UserContext() =>
        (User.FindFirstValue("acessoTotal") == "True",
         User.FindFirstValue("aplicacaoid"));

    [HttpGet]
    public IActionResult Get([FromQuery] string? aplicacaoid = null)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var appId = acessoTotal && !string.IsNullOrEmpty(aplicacaoid) ? aplicacaoid : claimAppId;

        var lista = _orcamentoRepo.Lista(appId!)
            .Select(o => new
            {
                o.Orcamentoid,
                o.Nome,
                o.Email,
                o.Telefone,
                o.Nomevendedor,
                o.Valorestimado,
                o.Prazo,
                o.Aprovado,
                o.Datainclusao
            });

        return Ok(lista);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();

        var orcamento = _orcamentoRepo.BuscaPorId(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        return Ok(new
        {
            orcamento.Orcamentoid,
            orcamento.Aplicacaoid,
            orcamento.Nome,
            orcamento.Email,
            orcamento.Telefone,
            orcamento.Descricaoservico,
            orcamento.Valorestimado,
            orcamento.Prazo,
            orcamento.Nomevendedor,
            orcamento.Aprovado,
            orcamento.Datainclusao,
            Itens = orcamento.OrcamentoDetalhes.Select(d => new
            {
                d.Orcamentodetalheid,
                d.Descricao,
                d.Quantidade,
                d.Valor,
                d.Ativo
            }).ToList()
        });
    }

    [HttpPut("{id}/aprovar")]
    public IActionResult Aprovar(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var orcamento = _orcamentoRepo.BuscaPorId(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        _orcamentoRepo.ToggleAprovado(orcamento);
        return Ok(new { orcamento.Aprovado });
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var (acessoTotal, claimAppId) = UserContext();
        var orcamento = _orcamentoRepo.BuscaPorId(id);
        if (orcamento == null) return NotFound();
        if (!acessoTotal && orcamento.Aplicacaoid != claimAppId) return Forbid();

        _orcamentoRepo.Remove(orcamento);
        return Ok();
    }
}
