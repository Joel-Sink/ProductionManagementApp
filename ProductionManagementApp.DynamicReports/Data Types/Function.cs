using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicReports.Data_Types
{
    public class Function
    {
        public string UpdateProp { get; set; }
        public List<FunctionVariable> Variables { get; set; }
        public List<Operator> Operators { get; set; }
        public dynamic CurrentValue { get; set; }
    }
}
