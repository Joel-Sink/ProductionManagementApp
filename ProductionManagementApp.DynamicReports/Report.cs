using Dapper;
using DynamicReports.Data_Types;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DynamicReports.GenericExtensions;

namespace DynamicReports
{
    public class Report
    {
        public List<string> Users { get; set; }
        public string Client { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public List<string> Lines { get; set; }
        public string FilePath { get; set; }
        List<string> Properties { get; set; }
        public List<Query> Queries { get; set; }
        public List<Function> Functions { get; set; }
        public List<string> Matches { get; set; }
        public List<Dictionary<int, QueryResult>> MatchedResults { get; set; }
        public List<ExpandoObject> Results { get; set; }

        public Report(string path, List<string> users, string client, DateTime start, DateTime stop)
        {
            Client = client;

            Users = users;

            StartTime = start;

            StopTime = stop;

            FilePath = path;

            LoadLines();

            GenerateProperties();

            GenerateQueries();

            GenerateFunctions();

            GenerateMatches();

            RunQueries();

            CheckUserIDResults();

            MatchResults();

            CalculateResults();
        }

        void LoadLines()
        {
            Lines = File.ReadAllLines(FilePath).ToList();
        }

        string GetConnectionString(string param)
        {
            return param switch
            {
                "DLX" => @"DATA SOURCE=ashprdwmrb-scan.gspt.net:1521/ASH_MTV2WMS_RPT;USER ID=MTV2_READONLY_USER;PASSWORD=mtv2r0user",
                "MAN" => @"DATA SOURCE=lvsprdwmoc-scan.gspt.net:1521/LVS_MTVAWMO_RPT;USER ID=mtvawmoprd;PASSWORD=mtva3wm0prd43",
                "SQLite" => @"Data Source=C:\Users\jcsin\Desktop\Test.db;Version=3;",
                _ => throw new Exception("Connection String Incorrectly Specified."),
            };
        }

        void GenerateQueries()
        {
            Lines.Where(i => i.Contains("QUERY")).ToList().ForEach(j =>
            {
                try
                {
                    string[] querystring = j.Replace("QUERY ", "").Split(' ');
                    var query = new Query()
                    {
                        SQL = File.ReadAllText(Directory.GetParent(FilePath).Parent.FullName + @"\Queries\" + querystring[0] + ".txt"),
                        ConnectionString = GetConnectionString(querystring[1]),
                        DatabaseType = querystring[2]
                    };

                    Queries.Add(query);
                }
                catch
                {
                    throw new Exception("Report not written correctly, please specify query name, connection string, " +
                        "and database type");
                }
            });
            ConfigureQueries();
        }

        void ConfigureQueries()
        {
            Queries.ForEach(i =>
            {
                i.SQL = i.SQL.Replace("@param0", StartTime.ToString("yyyyMMdd")).Replace("@param1", StopTime.ToString("yyyyMMdd"))
                    .Replace("@users", ConfigureUsers()).Replace("@badges", ConfigureBadges()).Replace("@client", Client);
            });
        }

        string ConfigureUsers()
        {
            return string.Concat(Users.Select((i) => i.Equals(Users.Last()) ? "'" + i.Split(',')[0] + "'" : "'" + i.Split(',')[0] + "', "));
        }

        string ConfigureBadges()
        {
            return string.Concat(Users.Select((i) => i.Equals(Users.Last()) ? "'" + i.Split(',')[2] + "'" : "'" + i.Split(',')[2] + "', "));
        }

        void GenerateFunctions()
        {
            Lines.Where(i => i.Contains("FUNCTION")).ToList().ForEach(j =>
            {
                string[] functionstring = j.Replace("FUNCTION ", "").Split(' ');

                Functions.Add(GetFunction(functionstring));                
            });
        }

        Function GetFunction(string[] functionstring)
        {
            string[] acceptedoperators = { "+", "-", "/", "*", "x" };

            Function function = new Function()
            {
                Variables = new List<FunctionVariable>(),
                Operators = new List<Operator>()
            };

            if(CheckPropertyValidity(functionstring[0], functionstring[1]))
            {
                function.UpdateProp = functionstring[0];
            }

            for (int i = 2; i < functionstring.Length; i++)
            {
                if (i % 2 != 0)
                {
                    if (acceptedoperators.Contains(functionstring[i]))
                    {
                        throw new Exception("Function not written properly, please do not use parenthesis");
                    }
                    else
                    {
                        function.Variables.Add(GetFunctionVariable(functionstring[i]));
                    }
                }
                else
                {
                    if (acceptedoperators.Contains(functionstring[i]))
                    {
                        function.Operators.Add(GetOperator(functionstring[i]));
                    }
                    else
                    {
                        throw new Exception("Function not written properly, please do not use parenthesis");
                    }
                }
            }

            return function;
        }

        bool CheckPropertyValidity(string property, string op)
        {
            if(op.Equals("="))
            {
                throw new Exception("Function must have an equals operator after the update property");
            }

            if(!Properties.Contains(property))
            {
                throw new Exception("Function must list a property to update, which must match a property defined in the report");
            }
            return true;
        }

        FunctionVariable GetFunctionVariable(string varible)
        {
            try
            {
                string[] varibledata = varible.Split('.');

                return new FunctionVariable(FunctionVariable.FunctionVariableType.QueryField)
                {
                    Query = int.Parse(varibledata[0]),
                    Column = varibledata[1]
                };
            }
            catch
            {
                try
                {
                    return new FunctionVariable(FunctionVariable.FunctionVariableType.Number) { Number = double.Parse(varible) };
                }
                catch
                {
                    throw new Exception("Function not written properly, please list varibles' query names followed by a period," +
                        "followed by the column");
                }
            }
        }

        Operator GetOperator(string op)
        {
            return op switch
            {
                "+" => Operator.ADD,
                "-" => Operator.SUBTRACT,
                "*" => Operator.MULTIPLY,
                "x" => Operator.MULTIPLY,
                "/" => Operator.DIVIDE,
                _ => throw new Exception()
            };
        }

        void GenerateMatches()
        {
            Matches = new List<string>();
            List<string> matches = Lines.Where(i => i.Contains("MATCH")).Select(i => i.Replace("MATCH ", "")).ToList();

            matches.ForEach(i =>
            {
                if(Queries.All(j => j.Results.First().Keys.Contains(i)))
                {
                    Matches.Add(i);
                }
                else
                {
                    throw new Exception("Report not written correctly, ensure all queries have result columns for all matches");
                }
            });
        }

        async void RunQueries()
        {
            List<Task> tasks = new List<Task>();

            Queries.ForEach(i =>
            {
                tasks.Add(Task.Run(() => 
                {
                    using (IDbConnection cnn = CreateConnection(i.DatabaseType, i.ConnectionString))
                    {
                        List<dynamic> results = cnn.Query(i.SQL).ToList();
                        i.Results = CreateResultDictionary(results);
                    }
                }));
            });

            await Task.WhenAll(tasks);
        }

        IDbConnection CreateConnection(string dbtype, string cns)
        {
            return dbtype switch
            {
                "Oracle" => new OracleConnection(cns),
                "SQLite" => new SQLiteConnection(cns),
                _ => throw new Exception("Report not written correctly, please specify query name, connection string, " +
                        "and database type")
            };
        }

        List<QueryResult> CreateResultDictionary(List<dynamic> results)
        {
            List<QueryResult> r = new List<QueryResult>();

            foreach (var result in results)
            {
                QueryResult pairs = new QueryResult();

                foreach(var prop in result.GetType().GetProperties())
                {
                    pairs.Add(prop.Name, prop.GetValue(result));
                }

                r.Add(pairs);
            }

            return r;
        }

        void CheckUserIDResults()
        {
            Queries.ForEach(i =>
            {
                if(CheckForUserColumn(i))
                {
                    string usercolumn = i.Results.First().Keys.Where(j => j.ToLower().Contains("us")).First();
                    string value = i.Results.First()[usercolumn];

                    if (Int32.TryParse(value, out int instr))
                    {
                        i.Results.ForEach(j =>
                        {
                            j[usercolumn] = Users
                            .Select(k => k.Split(','))
                            .Where(l => l[1].Equals(j[usercolumn]))
                            .First()[0];
                        });
                    }
                }
            });
        }

        bool CheckForUserColumn(Query query)
        {
            string[] usercolumnnames = { "Usr_id", "User", "User_id" };
            var keys = query.Results.First().Keys.ToList();

            foreach (var key in keys)
            {
                if (usercolumnnames.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        void MatchResults()
        {
            List<QueryResult> matchedvalues = FindMatchCriteria();

            MatchedResults = new List<Dictionary<int, QueryResult>>();

            foreach (var value in matchedvalues)
            {
                Dictionary<int, QueryResult> ob = new Dictionary<int, QueryResult>();
                for (int i = 0; i < Queries.Count; i++)
                {
                    ob.Add(i, Queries[i].Results.Where(j =>
                    {
                        foreach(var match in Matches)
                        {
                            if (j[match] != value[match])
                            {
                                return false;
                            }
                        }

                        return true;
                    }).FirstOrDefault());
                }
                MatchedResults.Add(ob);
            }
        }

        List<QueryResult> FindMatchCriteria()
        {
            List<QueryResult> matchedvalues = new List<QueryResult>();

            foreach (var query in Queries)
            {
                foreach (var result in query.Results)
                {
                    QueryResult values = new QueryResult();

                    foreach (var match in Matches)
                    {
                        values.Add(match, result[match]);
                    }

                    if (!matchedvalues.Contains(values))
                    {
                        matchedvalues.Add(values);
                    }
                }
            }

            return matchedvalues;
        }

        void CalculateResults()
        {
            if (!MatchedResults.IsEmpty())
            {
                foreach (var match in MatchedResults)
                {
                    var obj = new ExpandoObject() as IDictionary<string, dynamic>;

                    foreach (var property in Properties)
                    {
                        if (Matches.Contains(property))
                        {
                            obj.Add(property, match[0][property]);
                        }
                        else if (!Functions.Select(i => i.UpdateProp).Contains(property))
                        {
                            obj.Add(property, match.Where(i => i.Value.Keys.Contains(property)).First().Value[property]);
                        }
                        else
                        {
                            obj.Add(property, CalculateProperty(property, match));
                        }
                    }

                    Results.Add(obj as ExpandoObject);
                }
            }
            else if (Queries.Count == 1)
            {
                foreach (var result in Queries[0].Results)
                {
                    var obj = new ExpandoObject() as IDictionary<string, dynamic>;
                    foreach (var property in Properties)
                    {
                        obj.Add(property, result[property]);
                    }
                    Results.Add(obj as ExpandoObject);
                }
            }
        }

        void GenerateProperties()
        {
            string[] array = File.ReadAllLines(FilePath).Where(i => i.Contains("PROPERTY")).Select(i => i.Replace("PROPERTY ", "")).ToArray();

            if (array.All(i => !i.Contains('.')) && array.All(i => !i.Contains(',')) && array.All(i => !i.Contains(' ')))
            {
                Properties = array.ToList();
            }
            else
            {
                throw new Exception("Report not written correctly, please do not include periods, commas, or spaces in Property Definition.");
            }
        }

        dynamic CalculateProperty(string property, Dictionary<int, QueryResult> result)
        {
            var func = Functions.Where(i => i.UpdateProp.Equals(property)).FirstOrDefault();

            func.CurrentValue = GetFunctionVariableValue(func.Variables[0], result);

            for(int i = 0; i < func.Operators.Count; i++)
            {
                func.CurrentValue = DoMath(func.CurrentValue, func.Operators[i], func.Variables[i + 1], result);
            }

            return func.CurrentValue;
        }

        dynamic GetFunctionVariableValue(FunctionVariable variable, Dictionary<int, QueryResult> result)
        {
            return variable.FunctionType == FunctionVariable.FunctionVariableType.Number ? variable.Number :
                result[variable.Query][variable.Column];
        }

        object DoMath(dynamic currentValue, Operator op, FunctionVariable next, Dictionary<int, QueryResult> result)
        {
            try
            {
                return op switch
                {
                    Operator.ADD => currentValue + GetFunctionVariableValue(next, result),
                    Operator.SUBTRACT => currentValue - GetFunctionVariableValue(next, result),
                    Operator.MULTIPLY => currentValue * GetFunctionVariableValue(next, result),
                    Operator.DIVIDE => currentValue / GetFunctionVariableValue(next, result),
                    _ => throw new Exception("A function is not coded properly")
                };
            }
            catch (Exception e)
            {
                throw e;
            }

        }

    }
}
