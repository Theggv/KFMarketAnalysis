using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using KFMarketAnalysis.Models.Utility;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Prism.Mvvm;
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
    /// <summary>
    /// Базовый класс, представляющий LootBox.
    /// </summary>
    public abstract class LootBoxBase : BindableBase, ILootBox
    {
        private LootBoxState state;

        protected BitmapImage icon;

        protected string spanPattern = @"<br><span\sstyle=.{1,2}color:\s(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,45})<.{1,2}span>";
        protected string fontPattern = @"<br><font\scolor=.{1,2}(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,45})<.{1,2}font>";
        

        public string Name { get; set; }


        public double Profit => Items.ToArray().Where(x => x.Price > 0)?.Sum(item => item.Price) * 0.87 ?? 0;

        public double ProfitWithBundle => Profit - 102.5 * Count;

        public double ProfitWithoutBundle => Profit - 170 * Count;


        public int Count => Items.Count;


        public List<MarketItem> Items { get; set; } = new List<MarketItem>();


        public List<Description> Description { get; set; } = new List<Description>();


        [JsonIgnore]
        public BitmapImage Icon
        {
            get => icon;
            set
            {
                icon = value;
                icon.Freeze();

                RaisePropertyChanged("OnIconLoaded");
            }
        }

        public string IconUri
        {
            get => Icon?.UriSource?.ToString();
            set => Icon = new BitmapImage(new Uri(value));
        }

        public LootBoxState State
        {
            get => state;
            set
            {
                state = value;

                RaisePropertyChanged(nameof(State));
            }
        }


        public LootBoxBase() { }

        public LootBoxBase(ILootBox lootBox)
        {
            Name = lootBox.Name;
            Items = lootBox.Items;
            IconUri = lootBox.IconUri;
            Description = lootBox.Description;
            State = lootBox.State;

            foreach (var item in Items)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "OnPriceLoaded")
                        RaisePropertyChanged("OnPriceLoaded");
                };
            }
        }

        public LootBoxBase(string name)
        {
            Name = name;
        }


        public void AddItem(IMarketItem item)
        {
            item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnPriceLoaded")
                    RaisePropertyChanged("OnPriceLoaded");
            };

            Items.Add(item as MarketItem);

            RaisePropertyChanged("Items");
        }


        public virtual void GetIcon(string code, string name)
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.WithoutDelay, () =>
            {
                Icon = RequestsUtil.GetImageLootBox(code, name);

                if (Icon == null)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }

        
        public virtual void LoadDescription()
        {
            State = LootBoxState.Queue;

            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.High, async () =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest
                    .Create(RequestBuilder.ItemRequest(Name));

                if (ProxySingleton.GetInstance().CanUse)
                    request.Proxy = ProxySingleton.GetInstanceNext().Proxy;

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

                var description = json["assets"][0][0][0].GetArray("descriptions")[0].GetValue("value").ToString();

                description = HttpUtility.HtmlDecode(description).Replace("\"", "\\");


                string pattern;

                if (Regex.IsMatch(description, spanPattern))
                    pattern = spanPattern;
                else if (Regex.IsMatch(description, fontPattern))
                    pattern = fontPattern;
                else
                    return false;

                Regex regex = new Regex(pattern);

                Match match = regex.Match(description);

                Description = new List<Description>();

                while (match.Success)
                {
                    Description.Add(new Description(match.Groups[2].Value)
                        .WithColor(match.Groups[1].Value));

                    match = match.NextMatch();
                }

                State = LootBoxState.DescriptionLoaded;

                LoadItems();

                return true;
            });
        }

        public virtual void Update()
        {
            if(State <= LootBoxState.Queue)
            {
                LoadDescription();
            }
            else if (State <= LootBoxState.LoadStarted)
            {
                Items.Clear();
                LoadItems();
            }
            else
            {
                LoadPrices();
            }
        }
        
        public abstract void LoadItems();

        protected void LoadPrices()
        {
            foreach (var item in Items)
            {
                item.GetPrice();
            }
        }
    }
}
