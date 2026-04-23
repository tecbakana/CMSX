using System;
using System.Collections.Generic;
using CMSXData.Models;

namespace ICMSX
{
    public interface IClienteLojaDAL
    {
        void MakeConnection(dynamic prop);
        void CriaClienteLoja(Guid clienteLojaid, Guid aplicacaoid, int salematicClienteId);
        IEnumerable<ClienteLoja> ListaClienteLoja();
    }
}
