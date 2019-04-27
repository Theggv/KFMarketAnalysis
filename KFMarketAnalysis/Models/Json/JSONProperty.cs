using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Json
{
    [DebuggerDisplay("{Key}")]
    public class JSONProperty : JSONObject
    {
        public string Key { get; set; }

        public JSONObject Value { get; set; }

        public JSONProperty(string json)
        {
            for (int i = 0; i < json.Length; ++i)
            {
                if (json[i] == ':')
                {
                    Key = json.Substring(0, i).Replace("\"", "").Trim();

                    var value = json.Substring(i + 1).Trim();

                    if (value[0] == '{')
                        Value = new JSONObject(value);
                    else if (value[0] == '[')
                        Value = new JSONArray(value);
                    else
                        Value = new JSONElement(value);

                    break;
                }
            }
        }
    }
}
