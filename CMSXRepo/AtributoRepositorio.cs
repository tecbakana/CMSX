using System;
using System.Collections.Generic;
using System.Dynamic;
using CMSXData.Models;
using ICMSX;

namespace CMSXRepo
{
    public class AtributoRepositorio : BaseRepositorio, IAtributoRepositorio
    {
        private readonly IAtributoDAL _dal;

        public AtributoRepositorio(CmsxDbContext db, IAtributoDAL dal) : base(db) { _dal = dal; }

        public void MakeConnection(dynamic prop) => _dal.MakeConnection((ExpandoObject)prop);

        public List<Atributo> ListaAtributo() => throw new NotImplementedException();
        public List<Atributo> ListaAtributoXProduto() => throw new NotImplementedException();
        public void CriaAtributo(Atributo at) => throw new NotImplementedException();
        public void InativaAtributo() => throw new NotImplementedException();

        public List<Atributo> ListaAtributosArvore(IEnumerable<string> produtoIds)
        {
            var produtoList = produtoIds.ToList();
            var todos = new List<Atributo>();

            var raizes = _db.Atributos
                .Where(a => a.Produtoid != null && produtoList.Contains(a.Produtoid))
                .ToList();

            todos.AddRange(raizes);

            var idsParaBuscar = raizes.Select(a => a.Atributoid).ToList();
            while (idsParaBuscar.Count > 0)
            {
                var filhos = _db.Atributos
                    .Where(a => a.ParentAtributoId.HasValue && idsParaBuscar.Contains(a.ParentAtributoId.Value))
                    .ToList();

                if (filhos.Count == 0) break;

                todos.AddRange(filhos);
                idsParaBuscar = filhos.Select(a => a.Atributoid).ToList();
            }

            return todos;
        }
    }
}