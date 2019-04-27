using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models.Json
{
    public static class JSONParser
    {
        public static JSONObject Parse(string json)
        {
            json = json.Replace("\r", "").Replace("\n", "").Trim();

            if (json[0] == '{')
                return new JSONObject(json);
            else
                return new JSONArray(json);
        }
    }
}
