using CMSXData.Models;

namespace ICMSX
{
    public interface IAtributoRepositorio
    {
        void MakeConnection(dynamic prop);
        List<Atributo> ListaAtributo();
        List<Atributo> ListaAtributoXProduto();
        void CriaAtributo(Atributo at);
        void InativaAtributo();

        /// <summary>
        /// Retorna todos os atributos (raízes + descendentes) para os produtos informados, lista plana.
        /// </summary>
        List<Atributo> ListaAtributosArvore(IEnumerable<string> produtoIds);
    }
}