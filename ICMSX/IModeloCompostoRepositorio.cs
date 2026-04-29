using CMSXData.Models;

namespace ICMSX;

public interface IModeloCompostoRepositorio
{
    IEnumerable<ModeloComposto> ListarPorProduto(string aplicacaoid, string produtoid);
    ModeloComposto? BuscarPorHash(string hash, string aplicacaoid, string produtoid);
    void CriarOuIncrementar(ModeloComposto modelo, IEnumerable<ModeloSelecao> selecoes);
}
