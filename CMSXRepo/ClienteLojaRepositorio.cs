using System;
using System.Collections.Generic;
using System.Dynamic;
using CMSXData.Models;
using ICMSX;

namespace CMSXRepo
{
    public class ClienteLojaRepositorio : BaseRepositorio, IClienteLojaRepositorio
    {
        private readonly IClienteLojaDAL _dal;

        public ClienteLojaRepositorio(CmsxDbContext db, IClienteLojaDAL dal) : base(db) { _dal = dal; }

        public void MakeConnection(dynamic prop) => _dal.MakeConnection((ExpandoObject)prop);

        public void CriaClienteLoja(ClienteLoja cliente) =>
            _dal.CriaClienteLoja(
                Guid.NewGuid(),
                Guid.TryParse(cliente.Aplicacaoid, out var aid) ? aid : Guid.Empty,
                cliente.SalematicClienteId);

        public IEnumerable<ClienteLoja> ListaClienteLoja() => _dal.ListaClienteLoja();
    }
}
