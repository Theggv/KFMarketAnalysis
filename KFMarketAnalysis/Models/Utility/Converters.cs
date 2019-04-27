using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Utility
{
    public static class Converters
    {
        public static double ConvertToDouble(string s)
        {
            if (s == null)
                return -1;

            var buf = s.Split(' ');
            
            return double.Parse(buf[0]);
        }

        public static string ConvertToPrice(double? d, bool isNeedToConvert = false)
        {
            if (!d.HasValue)
                d = 0;

            if (d == -1)
                return "No data";

            if (d == -2)
                return "Error";

            if(isNeedToConvert)
                return $"{Math.Round(d.Value, 2)} ({Math.Round(d.Value * 0.87, 2)}) руб.";

            return $"{Math.Round(d.Value, 2)} руб.";
        }
    }
}
