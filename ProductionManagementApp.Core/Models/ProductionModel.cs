using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionManagementApp.Core.Models
{
    public class ProductionModel
    {
        public int PickUnits { private get; set; }
        public int PackUnits { private get; set; }
        public int PackOrders { private get; set; }
        public int ReplensLps { private get; set; }
        public int RecLps { private get; set; }
        public int FwdLps { private get; set; }
        public int RsvLps { private get; set; }

        public string Date { get; set; } 
        public string User { get; set; } 
        public double PickUPH { get; set; } 
        public double PackOPH { get; set; } 
        public double ReplensLPH { get; set; } 
        public double RecLPH { get; set; }
        public double FwdLPH { get; set; }
        public double RsvLPH { get; set; }

        private double _downtime;
        public double Downtime
        {
            get => _downtime;
            set
            {
                if (value > 6 * 60)
                {
                    _downtime = Math.Round((value - 60) / 60, 2);
                }
                else if (value > 4 * 60)
                {
                    _downtime = Math.Round((value - 30) / 60, 2);
                }
                else if (value > 2 * 60)
                {
                    _downtime = Math.Round((value - 15) / 60, 2);
                }
                else
                {
                    _downtime = Math.Round(value / 60, 2);
                }

                if (Downtime > Production_Hours)
                {
                    DowntimePercentage = Math.Round((double)100, 2) + "%";
                }
            }
        }

        public double Production_Hours { get; set; }

        private string _downtimePercentage;
        public string DowntimePercentage
        {
            get => _downtimePercentage;
            set
            {
                if (double.Parse(value.Remove(value.Length - 1)) > 100)
                {
                    _downtimePercentage = Math.Round((double)100, 2) + "%";
                }
                else
                {
                    _downtimePercentage = value;
                }
            }
        }


        public ProductionModel(string user, string date)
        {
            User = user;
            Date = date;
            PickUnits = 0;
            PackUnits = 0;
            PackOrders = 0;
            ReplensLps = 0;
            RecLps = 0;
            FwdLps = 0;
            RsvLps = 0;
            Downtime = 0;
        }   

        public void AddETime(HoursModel hours)
        {
            var pickhours = (double)hours.PickMinutes / 60;
            var packhours = (double)hours.PackMinutes / 60;
            var replenshours = (double)hours.ReplensMinutes / 60;
            var rechours = (double)hours.RecMinutes / 60;
            var fwdhours = (double)hours.FwdMinutes / 60;
            var rsvhours = (double)hours.RsvMinutes / 60;

            var totalhours = pickhours + packhours + replenshours + rechours + fwdhours + rsvhours;
            Production_Hours = Math.Round(totalhours, 2);

            DowntimePercentage = Math.Round(Downtime / totalhours  * 100, 2) + "%";

            PickUPH = Math.Round(PickUnits / pickhours, 2);
            PackOPH = Math.Round(PackOrders / packhours, 2); 
            ReplensLPH = Math.Round(ReplensLps / replenshours, 2);
            RecLPH = Math.Round(RecLps / rechours, 2); 
            FwdLPH = Math.Round(FwdLps / fwdhours, 2); 
            RsvLPH = Math.Round(RsvLps / rsvhours, 2); 
        }

	}
}
