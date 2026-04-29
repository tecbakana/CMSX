using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoCompostoRepositorio
{
    IEnumerable<OrcamentoDetalheComposto> ListarAtuais(Guid orcamentoid);
    Produto? BuscarProduto(string produtoid);
    IEnumerable<Opcao> BuscarOpcoes(IEnumerable<string> opcaoIds);
    IEnumerable<Atributo> BuscarAtributos(IEnumerable<Guid> atributoIds);
    void Criar(OrcamentoDetalheComposto detalhe, IEnumerable<Selecao> selecoes);
    void RemoverPorOrcamento(Guid orcamentoid);
}
