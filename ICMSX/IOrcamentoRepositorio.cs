using CMSXData.Models;

namespace ICMSX;

public interface IOrcamentoRepositorio
{
    IEnumerable<OrcamentoCabecalho> Lista(string aplicacaoid);
    OrcamentoCabecalho? BuscaPorId(Guid id);
    void Criar(OrcamentoCabecalho cabecalho, IEnumerable<OrcamentoDetalhe> itens);
    IEnumerable<Produto> ListaProdutosPublicos(string aplicacaoid);
    void ToggleAprovado(OrcamentoCabecalho orcamento);
    void Remove(OrcamentoCabecalho orcamento);
}
