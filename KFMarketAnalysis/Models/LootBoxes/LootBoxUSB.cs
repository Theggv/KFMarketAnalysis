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
using System.Web;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.LootBoxes
{
    public class LootBoxUSB : LootBoxBase
    {
        public LootBoxUSB() : base() { }

        public LootBoxUSB(string name) : base(name) { }

        
        public override void LoadItems()
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, () =>
            {
                RaisePropertyChanged("OnLoadStarted");

                return Task.FromResult(true);
            }, false);

            foreach (var description in Description)
            {
                if (description.Text.Contains("Exceedingly"))
                    continue;

                RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, async () =>
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest
                        .Create(RequestBuilder.SearchRequest(description.Text));

                    if (ProxySingleton.GetInstanceNext().CanUse)
                        request.Proxy = ProxySingleton.GetInstance().Proxy;

                    HttpWebResponse response = await RequestsUtil.GetResponseAsync(request);

                    await Task.Delay(RequestHandler.Delay);

                    if (response == null)
                        return false;

                    Stream stream = response.GetResponseStream();
                    StreamReader streamReader = new StreamReader(stream);

                    string content = streamReader.ReadToEnd();

                    response.Close();

                    JSONObject json = JSONParser.Parse(content);

                    var results = json.GetArray("results");

                    if (results == null)
                        return false;

                    string hashName, imageCode;
                    IMarketItem item;

                    for (int i = 0; i < results.Count; i++)
                    {
                        hashName = results[i]["asset_description"].GetValue("market_hash_name").ToString();

                        if (!hashName.ToLower().Contains(description.Text.ToLower()) ||
                            Items.Where(x => x.Name == hashName).Count() > 0)
                        {
                            continue;
                        }

                        item = new MarketItem(hashName);

                        AddItem(item);

                        imageCode = results[i]["asset_description"].GetValue("icon_url").ToString();

                        item.GetIcon(imageCode, hashName, Name);

                        RaisePropertyChanged("OnItemLoaded");
                    }
                    return true;
                });
            }

            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, () =>
            {
                RaisePropertyChanged("OnLoadCompleted");

                IsItemsListLoaded = true;
                LoadPrices();

                return Task.FromResult(true);
            }, false);
        }
    }
}
