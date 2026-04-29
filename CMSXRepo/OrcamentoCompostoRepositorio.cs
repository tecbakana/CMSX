using CMSXData.Models;
using ICMSX;
using Microsoft.EntityFrameworkCore;

namespace CMSXRepo;

public class OrcamentoCompostoRepositorio : BaseRepositorio, IOrcamentoCompostoRepositorio
{
    public OrcamentoCompostoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<OrcamentoDetalheComposto> ListarAtuais(Guid orcamentoid) =>
        _db.OrcamentoDetalheCompostos
            .Include(d => d.Selecoes)
            .Where(d => d.Orcamentoid == orcamentoid && d.Atual)
            .ToList();

    public Produto? BuscarProduto(string produtoid) =>
        _db.Produtos.FirstOrDefault(p => p.Produtoid == produtoid);

    public IEnumerable<Opcao> BuscarOpcoes(IEnumerable<string> opcaoIds)
    {
        var ids = opcaoIds.ToList();
        return _db.Opcaos.Where(o => ids.Contains(o.Opcaoid)).ToList();
    }

    public IEnumerable<Atributo> BuscarAtributos(IEnumerable<Guid> atributoIds)
    {
        var ids = atributoIds.ToList();
        return _db.Atributos.Where(a => ids.Contains(a.Atributoid)).ToList();
    }

    public void Criar(OrcamentoDetalheComposto detalhe, IEnumerable<Selecao> selecoes)
    {
        _db.OrcamentoDetalheCompostos.Add(detalhe);
        _db.Selecaos.AddRange(selecoes);
        _db.SaveChanges();
    }

    public void RemoverPorOrcamento(Guid orcamentoid)
    {
        var detalhes = _db.OrcamentoDetalheCompostos
            .Include(d => d.Selecoes)
            .Where(d => d.Orcamentoid == orcamentoid)
            .ToList();

        foreach (var d in detalhes)
            _db.Selecaos.RemoveRange(d.Selecoes);

        _db.OrcamentoDetalheCompostos.RemoveRange(detalhes);
        _db.SaveChanges();
    }
}
