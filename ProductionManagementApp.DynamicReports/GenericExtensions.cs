using System;
using System.Collections.Generic;

namespace DynamicReports
{
    public static class GenericExtensions
    {
        public static bool IsEmpty<T>(this List<T> input)
        {
            return input.Equals(new List<T>());   
        }

    }
}
