﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.Entity;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Configuration;
using ICMS;
using CMSXEF;

namespace CMSBLL
{
    public class DataFactory : IDataFactory
    {
        public int NumParms{get;set;}
        #region IDataFactory Members

        public DataFactory()
        {

        }

        public KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>> GetConnection(string banco, int nparm)
        {
            NumParms = nparm;

            switch (banco)
            {
                case "MySql":
                    return MySqlCon();
                case "EntityDB":
                    return EntityCon();
                default:
                    return SqlServerCon();
            }
        }

        public KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>> SqlServerCon()
        {
            var conn = new SqlConnection();
            var cmd = new SqlCommand();
            var parm = new List<SqlParameter>();

            for (int x = 0; x < NumParms; x++)
            {
                parm.Add(new SqlParameter());
            }

            conn.ConnectionString = System.Configuration.ConfigurationManager.AppSettings["sqlserverconn"].ToString();

            return new KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>>(conn, new KeyValuePair<IDbCommand, IDataParameter[]>(cmd, parm.ToArray()));
        }

        public KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>> MySqlCon()
        {
            var conn = new MySqlConnection();
            var cmd  = new MySqlCommand();
            var parm = new List<MySqlParameter>();

            for (int x = 0; x < NumParms; x++)
            {
                parm.Add(new MySqlParameter());
            }

            conn.ConnectionString = System.Configuration.ConfigurationManager.AppSettings["mysqlconn"].ToString();
            return new KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>>(conn, new KeyValuePair<IDbCommand, IDataParameter[]>(cmd, parm.ToArray()));

        }


        public KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>> EntityCon()
        {
            var conn = new cmsxDBEntities().Database.Connection;
            var cmd = conn.CreateCommand();
            var parm = new List<SqlParameter>();

            for (int x = 0; x < NumParms; x++)
            {
                parm.Add(new SqlParameter());
            }

            conn.ConnectionString = System.Configuration.ConfigurationManager.AppSettings["entityCon"].ToString();
            return new KeyValuePair<IDbConnection, KeyValuePair<IDbCommand, IDataParameter[]>>(conn, new KeyValuePair<IDbCommand, IDataParameter[]>(cmd, parm.ToArray()));
        }
        #endregion

    }
}
