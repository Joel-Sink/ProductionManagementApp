using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProductionManagementApp.Core.Queries
{
    public static class SQLiteQuery
    {
        public static List<T> LoadResults<T>(string sql, DynamicParameters parameters, string db)
        {
            using (IDbConnection cnn = new SqliteConnection(db))
            {
                cnn.Open();
                return cnn.Query<T>(sql, parameters).AsList();
            }
        }
    }
}
