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

        private MvxInteraction<List<string>> _reports = 
            new MvxInteraction<List<string>>();
        public MvxInteraction<List<string>> Reports => _reports;
        public string User { get; set; }
        public List<string> Users { get; set; }

        public MvxCommand LoadReports { get; set; }
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
            LoadReports = new MvxCommand(() => DoLoadReports());
        }

        void DoLoadReports()
        {
            Reports.Raise(FindReports());
        }

        public override void Prepare(ReportUserModel parameter)
        {
            Parameter = parameter;
        }

        List<string> FindReports()
        {
            return Directory.GetFiles(
                Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\ProductionManagementApp.DynamicReports\Reports")
                .Select(i => i.Replace(".txt", "")).ToList();
        }
    }
}
