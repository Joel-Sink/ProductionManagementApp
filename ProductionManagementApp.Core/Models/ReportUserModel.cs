using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionManagementApp.Core.Models
{
    public class ReportUserModel
    {
        public string UserId { get; set; }
        public List<string> Users { get; set; }
    }
}
