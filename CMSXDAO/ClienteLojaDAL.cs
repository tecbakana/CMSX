using System;
using System.Collections.Generic;
using System.Data;
using ICMSX;
using CMSXData.Models;

namespace CMSXDAO
{
    public class ClienteLojaDAL : BaseDAL, IClienteLojaDAL
    {
        public ClienteLojaDAL(IDataFactory factory) : base(factory) { }

        public void CriaClienteLoja(Guid clienteLojaid, Guid aplicacaoid, int salematicClienteId)
        {
            cmd.CommandText =
                "INSERT INTO clienteloja (ClienteLojaId, AplicacaoId, SalematicClienteId, DataInclusao) " +
                "VALUES (@id, @appid, @salematicId, GETDATE())";

            parms[0].ParameterName = "@id";         parms[0].Value = clienteLojaid;
            parms[1].ParameterName = "@appid";       parms[1].Value = aplicacaoid.ToString();
            parms[2].ParameterName = "@salematicId"; parms[2].Value = salematicClienteId;

            for (int i = 0; i <= 2; i++) cmd.Parameters.Add(parms[i]);
            cmd.Connection = conn;

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (DataException ex) { throw new DataException(ex.Message); }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { conn.Close(); }
        }

        public IEnumerable<ClienteLoja> ListaClienteLoja()
        {
            cmd.CommandText =
                "SELECT ClienteLojaId, AplicacaoId, SalematicClienteId, DataInclusao " +
                "FROM clienteloja WHERE AplicacaoId = @appid";

            parms[0].ParameterName = "@appid";
            parms[0].Value = _localProps.appid;
            cmd.Parameters.Add(parms[0]);
            cmd.Connection = conn;

            var lista = new List<ClienteLoja>();

            try
            {
                conn.Open();
                IDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(new ClienteLoja
                    {
                        ClienteLojaid      = dr["ClienteLojaId"].ToString(),
                        Aplicacaoid        = dr["AplicacaoId"].ToString(),
                        SalematicClienteId = Convert.ToInt32(dr["SalematicClienteId"]),
                        Datainclusao       = dr["DataInclusao"] == DBNull.Value ? null : Convert.ToDateTime(dr["DataInclusao"])
                    });
                }
            }
            catch (DataException ex) { throw new DataException(ex.Message); }
            catch (Exception ex) { throw new Exception(ex.Message); }
            finally { conn.Close(); }

            return lista;
        }
    }
}
