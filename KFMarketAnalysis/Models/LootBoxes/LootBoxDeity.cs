﻿using KFMarketAnalysis.Models.Interfaces;
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
    public class LootBoxDeity: LootBoxBase
    {
        public LootBoxDeity(): base() { }

        public LootBoxDeity(LootBoxBase lootBox) : base(lootBox) { }

        public LootBoxDeity(string name) : base(name) { }


        public override void LoadItems()
        {
            var temp = Description
                    .Select(str => str.Text.Split('|')[0].Trim()).ToList();

            var skins = new List<string>();

            for (int i = 0; i < temp.Count; i++)
            {
                string item = temp[i];

                if (!skins.Contains(item))
                    skins.Add(item);
            }

            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, () =>
            {
                return Task.Run(() =>
                {
                    State = LootBoxState.LoadStarted;

                    return true;
                });
            }, false);

            foreach (var description in skins)
            {
                RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Medium, async () =>
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest
                    .Create(RequestBuilder.SearchRequest(description));

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

                        if (!hashName.ToLower().Contains(description.ToLower()) ||
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
