using Dapper;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using ProductionManagementApp.Core.Models;
using ProductionManagementApp.Core.Queries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionManagementApp.Core.ViewModels
{
    public class ReportViewModel : MvxNavigationViewModel<ReportUserModel>
    {
        enum System
        {
            DLX,
            MAN
        }

        private readonly string _path;

        private ReportUserModel _parameter;

        public ReportUserModel Parameter
        {
            get { return _parameter; }
            set 
            {
                _parameter = value;
                User = Parameter.UserId;
                Users = Parameter.Users;
                RaisePropertyChanged(() => User);
                RaisePropertyChanged(() => Users);
            }
        }

        private DateTime dtstdt;

        public DateTime DTSTDT
        {
            get { return dtstdt; }
            set 
            { 
                dtstdt = value;
                RaisePropertyChanged(() => DTSTDT);
            }
        }

        private DateTime dtspdt;

        public DateTime DTSPDT
        {
            get { return dtspdt; }
            set 
            {
                dtspdt = value;
                RaisePropertyChanged(() => DTSPDT);
            }
        }

        private List<ProductionModel> _histDTData;

        public List<ProductionModel> HistDTData
        {
            get { return _histDTData; }
            set 
            {
                _histDTData = value;
                RaisePropertyChanged(() => HistDTData);
            }
        }


        public string User { get; set; }
        public List<string> Users { get; set; }

        public MvxCommand HistoricalDowntime { get; set; }

        private string _num;

        public string Num
        {
            get { return _num; }
            set { _num = value; RaisePropertyChanged(() => Num); }
        }


        public ReportViewModel(IMvxLogProvider logProvider, IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            _path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.Core\Users\";
            HistoricalDowntime = new MvxCommand(async () => { await HistoricalDowntimeReport();});
        }

        public override void Prepare(ReportUserModel parameter)
        {
            Parameter = parameter;
        }

        public async Task HistoricalDowntimeReport()
        {
            await Task.Run(() => Num = "Loading...");
            var client = File.ReadAllLines(_path + User + ".txt")[0];
            Dictionary<string, string> queries;
            string db;

            System system;

            if (client.Contains("Home Depot"))
            {
                system = System.DLX;
                db = "DATA SOURCE=ashprdwmrb-scan.gspt.net:1521/ASH_MTV2WMS_RPT;USER ID=MTV2_READONLY_USER;PASSWORD=mtv2r0user";

            }
            else
            {
                system = System.MAN;
                db = "DATA SOURCE=lvsprdwmoc-scan.gspt.net:1521/LVS_MTVAWMO_RPT;USER ID=mtvawmoprd;PASSWORD=mtva3wm0prd43";
            }

            if (TestSystem(system))
            {
                
                queries = ConfigureOracleQueries(system, client);
                await RunQueries(queries, db);
                
            }
            else
            {
                Num = "Can only reun reports for users in one system.";
            }
        }

        Dictionary<string, string> ConfigureOracleQueries(System system, string client)
        {
            Dictionary<string, string> queries = new Dictionary<string, string>();
            queries.Add("REPLENS", "");
            queries.Add("FWD", "");
            queries.Add("RSV", "");
            queries.Add("REC", "");
            queries.Add("PACK", "");
            queries.Add("PICK", "");
            queries.Add("ME", "");

            string[] q;

            if (system.Equals(System.DLX))
            {
                q = new string[] { "DLXPICK", "DLXPACK", "DLXFWD", "DLXRSV", "DLXREPLENS", "DLXREC", "DLXDOWN", "ETIME" };
            }
            else
            {
                q = new string[] { "MANPICK", "MANPACK", "MANFWD", "MANRSV", "MANREPLENS", "MANREC", "MANDOWN", "ETIME" };
            }

            var param0 = DTSTDT.ToString("yyyyMMdd");
            var param1 = DTSPDT.ToString("yyyyMMdd");
            
            foreach (var s in q)
            {
                var querypath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName 
                    + @"\ProductionManagementApp.Core\Queries\";

                var query = File.ReadAllText(querypath + s + ".txt");

                query = query.Replace("@param0", param0).Replace("@param1", param1).Replace("@users", 
                    string.Concat(ConfigureUsers())).Replace("@badges", string.Concat(LoadBadges()).Replace("@client", client));

                queries[RemoveFirstChars(s, 3)] = query;
            }
            return queries;
        }

        List<string> ConfigureUsers()
        {
            return Users.Select((i) => i.Equals(Users.Last()) ? "'" + i.Split(',')[0] + "'" : "'" + i.Split(',')[0] + "', ").ToList();
        }

        List<string> LoadBadges()
        {
            return Users.Select((i) => i.Equals(Users.Last()) ? "'" + i.Split(',')[2] + "'" : "'" + i.Split(',')[2] + "', ").ToList();            
        }

        bool TestSystem(System system)
        {
            if (system == System.DLX)
            {
                return !Users.Contains("MAN");
            }
            else
            {
                return !Users.Contains("DLX");
            }
        }

        string RemoveFirstChars(string s, int count = 4)
        {
            if (s.Length > count)
            {

                return new string(new string(s.Reverse().ToArray()).Remove(s.Length - count).Reverse().ToArray());
            }
            throw new Exception();
        }

        public async Task RunQueries(Dictionary<string, string> queries, string db)
        {
            List<QueryResultModel> qResults = new List<QueryResultModel>();

            try
            {
                Task<List<PickModel>> pickTask = Task.Run(() => OracleQuery.LoadResults<PickModel>(queries["PICK"], new DynamicParameters(), db));
                Task<List<PackModel>> packTask = Task.Run(() => OracleQuery.LoadResults<PackModel>(queries["PACK"], new DynamicParameters(), db));
                Task<List<ReplensModel>> replenTask = Task.Run(() => OracleQuery.LoadResults<ReplensModel>(queries["REPLENS"], new DynamicParameters(), db));
                Task<List<RecModel>> recTask = Task.Run(() => OracleQuery.LoadResults<RecModel>(queries["REC"], new DynamicParameters(), db));
                Task<List<FwdModel>> fwdTask = Task.Run(() => OracleQuery.LoadResults<FwdModel>(queries["FWD"], new DynamicParameters(), db));
                Task<List<RsvModel>> rsvTask = Task.Run(() => OracleQuery.LoadResults<RsvModel>(queries["RSV"], new DynamicParameters(), db));
                Task<List<DowntimeModel>> downTask = Task.Run(() => OracleQuery.LoadResults<DowntimeModel>(queries["DOWN"], new DynamicParameters(), db));
                Task<List<HoursModel>> hoursTask = Task.Run(() => OracleQuery.LoadResults<HoursModel>(queries["ME"], new DynamicParameters(),
                    "DATA SOURCE=206.90.32.204:1630/etn01pdg;USER ID=q53dda;PASSWORD=q5_126"));

                await Task.WhenAll(pickTask, packTask, replenTask, recTask, fwdTask, rsvTask, downTask, hoursTask);

                pickTask.Result.ForEach((i) => qResults.Add(i));
                packTask.Result.ForEach((i) => qResults.Add(i));
                replenTask.Result.ForEach((i) => qResults.Add(i));
                recTask.Result.ForEach((i) => qResults.Add(i));
                fwdTask.Result.ForEach((i) => qResults.Add(i));
                rsvTask.Result.ForEach((i) => qResults.Add(i));
                downTask.Result.ForEach((i) => qResults.Add(i));
                hoursTask.Result.ForEach((i) =>
                {
                    i.Usr_id = Users.Where((j) => j.Contains(i.Usr_id)).First().Split(',')[0];
                    qResults.Add(i);
                });

                Num = pickTask.Result.First().Units + "";

                await Task.Run(() =>
                {
                    List<ProductionModel> data = new List<ProductionModel>();

                    foreach (var u in Users.Select((i) => i.Split(',')[0]))
                    {
                        List<QueryResultModel> uResults = new List<QueryResultModel>();
                        qResults.ForEach((i) =>
                        { if (i.Usr_id.Equals(u)) { uResults.Add(i); } });

                        List<string> dates = new List<string>();

                        foreach (var result in uResults)
                        {
                            var year = result.Year;
                            var month = result.Month;
                            var day = result.Day;
                            var date = $"{year:D4}{month:D2}{day:D2}";
                            if (!dates.Contains(date))
                                dates.Add(date);
                        }

                        foreach (var date in dates)
                        {
                            data.Add(CreateProductionModel(uResults, u, date));
                        }
                    }
                    HistDTData = data;
                });
            }
            catch
            {
                Num = "Oracle Query Could Not Be Exectued. If You Are Currently Connected To Readial's Network And Are Still Experiencing Issues, Please Contact Joel Sink";
            }
        }

        ProductionModel CreateProductionModel(List<QueryResultModel> model, string user, string date)
        {
            var usermatch = model.Where((i) =>
            {
                var year = i.Year;
                var month = i.Month;
                var day = i.Day;
                return $"{year:D4}{month:D2}{day:D2}".Equals(date);
            }).ToList();

            ProductionModel productionModel = new ProductionModel(user, date);

            usermatch.ForEach((i) =>
            {
                if (i.GetType() == typeof(PickModel))
                {
                    productionModel.PickUnits = ((PickModel)i).Units;
                }
                else if (i.GetType() == typeof(PackModel))
                {
                    productionModel.PackUnits = ((PackModel)i).Units;
                    productionModel.PackOrders = ((PackModel)i).Orders;
                }
                else if (i.GetType() == typeof(ReplensModel))
                {
                    productionModel.ReplensLps = ((ReplensModel)i).LPs;
                }
                else if (i.GetType() == typeof(RecModel))
                {
                    productionModel.RecLps = ((RecModel)i).LPs;
                }
                else if (i.GetType() == typeof(FwdModel))
                {
                    productionModel.FwdLps = ((FwdModel)i).LPs;
                }
                else if (i.GetType() == typeof(RsvModel))
                {
                    productionModel.RsvLps = ((RsvModel)i).LPs;
                }
                else if (i.GetType() == typeof(DowntimeModel))
                {
                    productionModel.Downtime = ((DowntimeModel)i).Downtime;
                }
                else
                {
                    productionModel.AddETime((HoursModel)i);
                }
            });

            return productionModel;

        }
    }
}
