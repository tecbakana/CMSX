using CMSXData.Models;
using ICMSX;

namespace CMSXRepo;

public class ModeloCompostoRepositorio : BaseRepositorio, IModeloCompostoRepositorio
{
    public ModeloCompostoRepositorio(CmsxDbContext db) : base(db) { }

    public IEnumerable<ModeloComposto> ListarPorProduto(string aplicacaoid, string produtoid) =>
        _db.ModeloCompostos
            .Where(m => m.Aplicacaoid == aplicacaoid && m.Produtoid == produtoid)
            .OrderByDescending(m => m.Usos)
            .ToList();

    public ModeloComposto? BuscarPorHash(string hash, string aplicacaoid, string produtoid) =>
        _db.ModeloCompostos.FirstOrDefault(m =>
            m.ConfiguracaoHash == hash &&
            m.Aplicacaoid == aplicacaoid &&
            m.Produtoid == produtoid);

    public void CriarOuIncrementar(ModeloComposto modelo, IEnumerable<ModeloSelecao> selecoes)
    {
        var existente = BuscarPorHash(modelo.ConfiguracaoHash, modelo.Aplicacaoid, modelo.Produtoid);

        if (existente != null)
        {
            existente.Usos++;
            _db.SaveChanges();
            return;
        }

        _db.ModeloCompostos.Add(modelo);
        _db.ModeloSelecaos.AddRange(selecoes);
        _db.SaveChanges();
    }
}
