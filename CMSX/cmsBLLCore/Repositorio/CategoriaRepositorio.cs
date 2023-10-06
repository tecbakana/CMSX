﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ICMSX;
using System.Linq;
using CMSXData;
using CMSXData.Models;
using System.Dynamic;

namespace CMSXBLL.Repositorio
{
    public class CategoriaRepositorio : BaseRepositorio, ICategoriaRepositorio
    {
        private ICategoriaDAL dal;

        #region IAplicacaoRepositorio Members

        public void MakeConnection(dynamic prop)
        {
            dal = container.Resolve<ICategoriaDAL>();
            db = new CmsxDbContext();
            string bc = prop.banco;
            int parm = prop.parms;
            lprop = prop;
            dal.MakeConnection((ExpandoObject)prop);
        }

        public List<Categoria> Helper(DataTable appdata)
        {
            if (appdata == null) return null;
            List<Categoria> applista = new List<Categoria>();
            if (appdata.TableName != "Categoria_full")
            {
                foreach (DataRow dr in appdata.Rows)
                {
                    Categoria _app = Categoria.ObterNovaCategoria();
                    _app.Nome = dr["Nome"].ToString();
                    _app.NomePai = dr["NomePai"].ToString();
                    _app.Descricao = dr["Descricao"].ToString();
                    _app.AplicacaoId = new System.Guid(dr["AplicacaoId"].ToString());
                    _app.CategoriaId = new System.Guid(dr["CategoriaId"].ToString());

                    if (dr.Table.Columns.Contains("CategoriaIdPai"))
                    {
                        _app.CategoriaIdPai = new System.Guid(dr["CategoriaIdPai"].ToString());
                    }

                    applista.Add(_app);
                }
            }
            else//adicionando subCategorias
            {
                foreach (DataRow dr in appdata.Rows)
                {
                    Categoria _cat      = Categoria.ObterNovaCategoria();
                    _cat.Nome           = dr["Nome"].ToString();
                    _cat.NomePai        = null;
                    _cat.Descricao      = dr["Descricao"].ToString();
                    _cat.AplicacaoId    = new System.Guid(dr["AplicacaoId"].ToString());
                    _cat.CategoriaId    = new System.Guid(dr["CategoriaId"].ToString());
                    _cat.CategoriaIdPai = null;// new System.Guid(dr["CategoriaIdPai"].ToString());
                    applista.Add(_cat);

                    if (!string.IsNullOrEmpty(dr["sCategoriaId"].ToString()))
                    {
                        //adicionando sub categorias
                        _cat = Categoria.ObterNovaCategoria();
                        _cat.Nome = dr["sNome"].ToString();
                        _cat.NomePai = dr["Nome"].ToString();
                        _cat.Descricao = dr["sDescricao"].ToString();
                        _cat.AplicacaoId = new System.Guid(dr["AplicacaoId"].ToString());
                        _cat.CategoriaId = new System.Guid(dr["sCategoriaId"].ToString());
                        _cat.CategoriaIdPai = new System.Guid(dr["sCategoriaIdPai"].ToString());
                        applista.Add(_cat);
                    }
                }
            }
            return applista;
        }

        public List<Categoria> Helper(IEnumerable<Categoria> entLista)
        {
            if (entLista == null) return null;
            List<Categoria> catLista = new List<Categoria>();

            foreach (Categoria cat in entLista)
            {
                Categoria _cat = Categoria.ObterNovaCategoria();
                _cat.CategoriaId = cat.CategoriaId;
                _cat.Nome = cat.Nome;
                _cat.Descricao = cat.Descricao;
                catLista.Add(_cat);
            }
            return catLista;
        }

        #endregion

        public Categoria ObtemCategoriaPorId()
        {
            return Helper(dal.ListaCategoriasPorId())[0];
        }

        public void CriaNovaCategoria()
        {
            dal.CriaCategoria();
        }

        public void CategoriaRapida()
        {
           /* var catObj = lprop.categoria;
            using (CmsxDbContext dbLoc = new CmsxDbContext())
            {

                var ncat = new Categoria();
                ncat.Nome = catObj.Nome;
                ncat.AplicacaoId = catObj.AplicacaoId.ToString();
                ncat.Descricao = catObj.Descricao;
                ncat.CategoriaId = catObj.CategoriaId.ToString();
                ncat.TipoCategoria = catObj.TipoCategoria;
                if (catObj.CategoriaIdPai != null)
                {
                    ncat.CategoriaIdPai = catObj.CategoriaIdPai.ToString();
                }

                dbLoc.Cateria.Add(ncat);
                dbLoc.SaveChanges();
            }   */
        }

        public List<Categoria> ListaCategoria()
        {
            return Helper(dal.ListaCategorias());
        }

        public List<Categoria> ListaCategoriaFull()
        {
            return Helper(dal.ListaCategoriasFull());
        }

        public List<Categoria> ListaCategoriaPai()
        {
            string appid = lprop.appid.ToString();
            IEnumerable<Categoria> lst = from cat in db.Cateria
                                         where ((cat.Aplicacaoid == appid)&&(cat.Cateriaidpai==null))
                                         select new Categoria()
                                         {
                                             CategoriaId = new Guid(cat.Cateriaid),
                                             CategoriaIdPai = new Guid(cat.Cateriaidpai),
                                             Nome = cat.Nome,
                                             Descricao = cat.Descricao,
                                             TipoCategoria = cat.Tipocateria??0,
                                             AplicacaoId = new Guid(cat.Aplicacaoid),
                                             NomePai = string.Empty
                                         };
            return Helper(lst);
        }

        public List<Categoria> ListaSubCategoria()
        {
            string cpid = lprop.cpid;
            IEnumerable<Categoria> lst = from cat in db.Cateria
                                         where cat.Cateriaidpai == cpid
                                         select new Categoria()
                                         {
                                             CategoriaId = new Guid(cat.Cateriaid),
                                             CategoriaIdPai = new Guid(cat.Cateriaidpai),
                                             Nome = cat.Nome,
                                             Descricao = cat.Descricao,
                                             TipoCategoria = cat.Tipocateria ?? 0,
                                             AplicacaoId = new Guid(cat.Aplicacaoid),
                                             NomePai = string.Empty
                                         };
            return Helper(lst);
        }

        public void InativaCategorias()
        {
            throw new NotImplementedException();
        }
    }
}
