using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionManagementApp.Core.Models
{
    public class PackModel : QueryResultModel
    {
        public int Orders { get; set; }
        public int Units { get; set; }
    }
}
