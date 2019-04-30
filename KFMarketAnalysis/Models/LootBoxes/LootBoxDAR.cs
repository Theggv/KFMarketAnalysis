using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using KFMarketAnalysis.Models.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.LootBoxes
{
    public class LootBoxDAR : LootBoxBase
    { 
        public LootBoxDAR(): base() { }

        public LootBoxDAR(LootBoxBase lootBox) : base(lootBox) { }

        public LootBoxDAR(string name): base(name) { }


        public override void LoadItems()
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, () =>
            {
                return Task.Run(() => 
                {
                    State = LootBoxState.LoadStarted;

                    return true;
                });
            }, false);

            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, async () =>
            {
                string skin = "D.A.R.";

                HttpWebRequest request = (HttpWebRequest)WebRequest
                    .Create(RequestBuilder.SearchRequest(skin));

                if (ProxySingleton.GetInstance().CanUse)
                    request.Proxy = ProxySingleton.GetInstance().Proxy;

                HttpWebResponse response = await RequestsUtil.GetResponseAsync(request);

                if (response == null)
                    return false;

                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);

                string content = streamReader.ReadToEnd();

                response.Close();

                JSONObject json = JSONParser.Parse(content);

                if (json.GetValue("success").ToString() == "false" ||
                    json.GetValue("total_count").ToString() == "0")
                    return false;

                var results = json.GetArray("results");

                if (results == null)
                    return false;

                string hashName, imageCode;
                IMarketItem item;

                for (int i = 0; i < results.Count; i++)
                {
                    hashName = results[i]["asset_description"].GetValue("market_hash_name").ToString();

                    if (hashName.Contains("Supply"))
                        continue;

                    item = new MarketItem(hashName);

                    AddItem(item);

                    imageCode = results[i]["asset_description"].GetValue("icon_url").ToString();

                    item.GetIcon(imageCode, hashName, Name);

                    RaisePropertyChanged("OnItemLoaded");
                }

                return true;
            });

            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, () =>
            {
                return Task.Run(() =>
                {
                    State = LootBoxState.ItemsLoaded;

                    LoadPrices();

                    return true;
                });
            }, false);
        }
    }
}
