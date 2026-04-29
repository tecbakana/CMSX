using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class OrcamentoRepositorio : BaseRepositorio, IOrcamentoRepositorio
{
    public OrcamentoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<OrcamentoCabecalho> Lista(string aplicacaoid) =>
        _db.OrcamentoCabecalhos
            .Where(o => o.Aplicacaoid == aplicacaoid)
            .OrderByDescending(o => o.Datainclusao)
            .ToList();

    public OrcamentoCabecalho? BuscaPorId(Guid id) =>
        _db.OrcamentoCabecalhos
            .Include(o => o.OrcamentoDetalhes)
            .FirstOrDefault(o => o.Orcamentoid == id);

    public void Criar(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens)
    {
        _db.OrcamentoCabecalhos.Add(cabecalho);
        _db.OrcamentoDetalhes.AddRange(itens);
        _db.SaveChanges();
    }

    public IEnumerable<Produto> ListaProdutosPublicos(string aplicacaoid) =>
        _db.Produtos
            .Where(p => p.Aplicacaoid == aplicacaoid)
            .ToList();

    public void ToggleAprovado(OrcamentoCabecalho orcamento)
    {
        orcamento.Aprovado = !orcamento.Aprovado;
        _db.SaveChanges();
    }

    public void Remove(OrcamentoCabecalho orcamento)
    {
        _db.OrcamentoDetalhes.RemoveRange(orcamento.OrcamentoDetalhes);
        _db.OrcamentoCabecalhos.Remove(orcamento);
        _db.SaveChanges();
    }
}
