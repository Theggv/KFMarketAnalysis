using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KFMarketAnalysis.Models
{
    public static class RequestBuilder
    {
        private const int appId = 232090;
        private const string marketUrl = "https://steamcommunity.com/market";

        public static string PriceRequest(string name)
        {
            name = NormalizeString(name);

            return $"{marketUrl}/priceoverview/?appid={appId}&country=ru&currency=5&market_hash_name={name}";
        }

        public static string SearchRequest(string name)
        {
            name = NormalizeString(name);

            return $"{marketUrl}/search/render/?query={name}&appid={appId}&start=0&count=100&currency=5&norender=1";
        }

        public static string ItemRequest(string name)
        {
            name = NormalizeString(name);

            return $"{marketUrl}/listings/{appId}/{name}/render?start=0&count=1&currency=5&language=russian&format=json&norender=1";
        }

        private static string NormalizeString(string s)
        {
            return Uri.EscapeDataString(s);
        }
    }
}
