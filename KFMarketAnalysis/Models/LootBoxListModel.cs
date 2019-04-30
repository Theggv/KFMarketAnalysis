using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using KFMarketAnalysis.Models.LootBoxes;
using KFMarketAnalysis.Models.Utility;
using KFMarketAnalysis.ViewModels;
using Newtonsoft.Json;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace KFMarketAnalysis.Models
{
    public class LootBoxListModel : BindableBase
    {
        private string dataPath = "data.json";

        private LootBoxListVM vm;

        public LootBoxListModel(LootBoxListVM vm)
        {
            this.vm = vm;
        }

        public void GetLootBoxes()
        {
            if(!Load())
                LoadLootBoxes();
        }

        private void LoadLootBoxes()
        {
            LoadBoxes("Crate");
            LoadBoxes("USB");
        }

        private void LoadBoxes(string name)
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.High, async () =>
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
            }, false);
        }

        public bool Load()
        {
            if (File.Exists(dataPath))
            {
                JsonSerializer serializer = new JsonSerializer();

                using (StreamReader sr = new StreamReader(dataPath))
                {
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        try
                        {
                            vm.LootBoxes = serializer.Deserialize<List<LootBoxUSB>>(reader)
                                .Select(x => new LootBoxVM(LootBoxFactory.RestoreLootBox(x)));
                        }
                        catch { return false; }
                    }
                }

                return true;
            }
            else
                return false;
        }

        public void Save(IEnumerable<LootBoxVM> lootBoxesVM)
        {
            using (StreamWriter sw = new StreamWriter(dataPath))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    JsonSerializer serializer = new JsonSerializer
                    {
                        Formatting = Formatting.Indented
                    };

                    serializer.Error += (src, ev) =>
                    {
                        Logging.AddToLog(src, ev.ErrorContext?.Error);
                    };

                    var lootBoxes = lootBoxesVM.Select(x => x.LootBox);

                    foreach (var item in lootBoxes)
                    {
                        if (item.State == LootBoxState.Queue)
                            item.State = LootBoxState.NotLoaded;
                        else if (item.State == LootBoxState.LoadStarted)
                            item.State = LootBoxState.DescriptionLoaded;
                    }

                    serializer.Serialize(sw, lootBoxes.ToList());
                }
            }
        }
    }
}
