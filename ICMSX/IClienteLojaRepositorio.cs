using System.Collections.Generic;
using CMSXData.Models;

namespace ICMSX
{
    public interface IClienteLojaRepositorio
    {
        void MakeConnection(dynamic prop);
        void CriaClienteLoja(ClienteLoja cliente);
        IEnumerable<ClienteLoja> ListaClienteLoja();
    }
}
