using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using KFMarketAnalysis.Models.LootBoxes;
using KFMarketAnalysis.Models.Utility;
using KFMarketAnalysis.ViewModels;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace KFMarketAnalysis.Models
{
    public class LootBoxListModel: BindableBase
    {
        private LootBoxListVM vm;
        private List<ILootBox> lootBoxes;

        public LootBoxListModel(LootBoxListVM vm)
        {
            this.vm = vm;

            lootBoxes = new List<ILootBox>();
        }

        public List<ILootBox> GetLootBoxes()
        {
            if (lootBoxes.Count == 0)
                LoadLootBoxes();

            return lootBoxes;
        }

        private void LoadLootBoxes()
        {
            //LoadBoxes("Crate");
            LoadBoxes("USB");
        }

        private void LoadBoxes(string name)
        {
            RequestConveyorSingleton.GetInstance().AddAction(RequestConveyorSingleton.Priority.Description, async () =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest
                    .Create(RequestBuilder.SearchRequest(name));

                HttpWebResponse response = await RequestsUtil.GetResponseAsync(request);

                if (response == null)
                    return false;

                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream);

                string content = streamReader.ReadToEnd();

                response.Close();

                JSONObject json = JSONParser.Parse(content);

                if (json.GetValue("success").ToString() == "false"
                || json.GetValue("total_count").ToString() == "0")
                    return false;

                var results = json.GetArray("results");

                for (int i = 0; i < results.Count; i++)
                {
                    var hashName = results[i]["asset_description"].GetValue("market_hash_name").ToString();

                    // skip keys
                    if (hashName.Contains("Key"))
                        continue;

                    var iconUrl = results[i]["asset_description"].GetValue("icon_url").ToString();

                    var lootBox = LootBoxFactory.GetLootBox(hashName);

                    lootBox.GetIcon(iconUrl, hashName);

                    vm.AddLootBox(lootBox);
                }

                return true;
            });
        }
    }
}
