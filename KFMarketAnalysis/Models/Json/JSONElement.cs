using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Json
{
    [DebuggerDisplay("{Value}")]
    public class JSONElement : JSONObject
    {
        public object Value { get; set; }

        public JSONElement(string json)
        {
            Value = json.Replace("\"", "").Trim();
        }
    }
}
