    using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace ProductionManagementApp.Core.Queries
{
    public static class OracleQuery
    { 
        public static List<T> LoadResults<T>(string sql, DynamicParameters parameters, string db)
        {
            using (IDbConnection cnn = new OracleConnection(db))
            {
                cnn.Open();
                return cnn.Query<T>(sql, parameters).AsList();
            }
        }
    }
}
