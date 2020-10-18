using System;
using System.Collections.Generic;
using System.Text;

namespace ProductionManagementApp.Core.Models
{
    public static class ModelExtensions
    {
        public static bool EqualsModel(this PickModel model, QueryResultModel test)
        {
            return model.Year == test.Year && model.Day == test.Day && model.Month == test.Month && model.Usr_id.Equals(test.Usr_id);
        }

    }
}
