using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicReports.Data_Types
{
    public class Query
    {
        public string SQL { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseType { get; set; }
        public List<QueryResult> Results { get; set; }
    }
}
