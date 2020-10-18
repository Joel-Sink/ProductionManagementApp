using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionManagementApp.Core.Models
{
    public class HoursModel : QueryResultModel
    {
        public int PickMinutes { get; set; }
        public int PackMinutes { get; set; }
        public int ReplensMinutes { get; set; }
        public int RecMinutes { get; set; }
        public int FwdMinutes { get; set; }
        public int RsvMinutes { get; set; }

    }
}
