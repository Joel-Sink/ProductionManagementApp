using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicReports.Data_Types
{
    public class FunctionVariable
    {
        public enum FunctionVariableType
        {
            QueryField,
            Number
        }
        public FunctionVariableType FunctionType { get; set; }
        public int Query { get; set; }
        public string Column { get; set; }
        public double Number { get; set; }

        public FunctionVariable(FunctionVariableType type)
        {
            FunctionType = type;
        }
    }
}
