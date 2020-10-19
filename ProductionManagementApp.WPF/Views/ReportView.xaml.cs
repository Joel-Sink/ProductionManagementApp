using MvvmCross.Base;
using MvvmCross.Binding.BindingContext;
using MvvmCross.ViewModels;
using ProductionManagementApp.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProductionManagementApp.WPF.Views
{
    /// <summary>
    /// Interaction logic for ReportView.xaml
    /// </summary>
    [MvxViewFor(typeof(ReportViewModel))]
    public partial class ReportView
    {

        private IMvxInteraction<List<string>> _reports;

        public IMvxInteraction<List<string>> Reports
        {
            get { return _reports; }
            set 
            {
                if (_reports != null)
                    _reports.Requested -= OnLoadReports;
                
                _reports = value;
                _reports.Requested += OnLoadReports;
            }
        }

            

        public ReportView()
        {
            var set = this.CreateBindingSet<ReportView, ReportViewModel>();

            set.Bind(this).For(view => view.Reports).To(viewmodel => viewmodel.LoadReports).OneWay();
            set.Apply();

            

            InitializeComponent();

            
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void OnLoadReports(object sender, MvxValueEventArgs<List<string>> args)
        {
            var items = new List<TabItem>();

            foreach(var report in args.Value)
            {
                items.Add(new TabItem
                {
                    Header = report
                });   
            }

            ReportTabs.ItemsSource = items;
        }
    }
}
