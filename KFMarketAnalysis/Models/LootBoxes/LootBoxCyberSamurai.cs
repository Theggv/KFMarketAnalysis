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
    public class LootBoxCyberSamurai: LootBoxBase
    {
      
        public LootBoxCyberSamurai() : base() { }

        public LootBoxCyberSamurai(string name) : base(name) { }


        public override void LoadItems()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest
                  .Create(RequestBuilder.ItemRequest(Name));

            if (ProxySingleton.GetInstance().CanUse)
                request.Proxy = ProxySingleton.GetInstance().Proxy;

            HttpWebResponse response = RequestsUtil.GetResponse(request);

            if (response == null)
                return;

            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);

            string content = streamReader.ReadToEnd();

            response.Close();

            JSONObject json = JSONParser.Parse(content);

            if (json.GetValue("success").ToString() == "false")
                return;

            var description = json["assets"][0][0][0].GetArray("descriptions")[0].GetValue("value").ToString();

            description = HttpUtility.HtmlDecode(description).Replace("\"", "\\");

            var pattern = @"<br><\w{4}\s\w{5}=.{1,2}.{0,6}\w{0,1}(#\w{6}).{1,2}>(.{1,50})<.{1,3}\w{4}>";

            if (Regex.IsMatch(description, pattern))
            {
                var skins = Regex
                    .Split(description, pattern)
                    .Where(str => str.Contains("|") || str.Trim() != "")
                    .Where(str => str[0] != '#' && !str.Contains("Precious") && !str.Contains("Contains")).ToList();

                var colors = Regex
                   .Split(description, pattern)
                   .Where(str => str.Length == 7 && str[0] == '#').ToList();

                Description = new List<Description>();

                for (int i = 0; i < skins.Count; i++)
                {
                    Description.Add(new Description(skins[i]).WithColor(colors[i]));
                }

                RaisePropertyChanged("OnDescriptionLoaded");

                var temp = skins
                    .Select(str => str.Split('|')[0].Trim()).ToList();

                skins = new List<string>();

                foreach (var item in temp)
                {
                    if (!skins.Contains(item))
                        skins.Add(item);
                }

                Task.Run(async () =>
                {
                    foreach (var skin in skins)
                    {
                        request = (HttpWebRequest)WebRequest
                            .Create(RequestBuilder.SearchRequest(skin));

                        if (ProxySingleton.GetInstance().CanUse)
                            request.Proxy = ProxySingleton.GetInstance().Proxy;

                        response = await RequestsUtil.GetResponseAsync(request);

                        await Task.Delay(4000);

                        if (response == null)
                            return;

                        stream = response.GetResponseStream();
                        streamReader = new StreamReader(stream);

                        content = streamReader.ReadToEnd();

                        response.Close();

                        json = JSONParser.Parse(content);

                        if (json.GetValue("total_count").ToString() == "0")
                            continue;

                        var results = json.GetArray("results");

                        string marketName, hashName, imageCode;
                        IMarketItem item;

                        for (int i = 0; i < results.Count; i++)
                        {
                            hashName = results[i]["asset_description"].GetValue("market_hash_name").ToString();
                            marketName = results[i]["asset_description"].GetValue("name").ToString();

                            if (!marketName.Contains(skin))
                                continue;

                            item = new MarketItem(hashName);

                            AddItem(item);

                            imageCode = results[i]["asset_description"].GetValue("icon_url").ToString();
                            
                            item.GetIcon(imageCode, hashName, Name);

                            item.GetPrice();
                            
                            RaisePropertyChanged("OnItemLoaded");
                        }
                    }

                    RaisePropertyChanged("OnLoadCompleted");
                });
            }
        }
    }
}
