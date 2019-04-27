using KFMarketAnalysis.Models;
using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Json;
using KFMarketAnalysis.Models.Utility;
using Prism.Mvvm;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.LootBoxes
{
    /// <summary>
    /// Базовый класс, представляющий LootBox.
    /// </summary>
    public abstract class LootBoxBase : BindableBase, ILootBox
    {
        protected BitmapImage icon;

        protected string spanPattern = @"<br><span\sstyle=.{1,2}color:\s(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,40})<.{1,2}span>";
        protected string fontPattern = @"<br><font\scolor=.{1,2}(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,50})<.{1,2}font>";

        public string Name { get; set; }

        public double Profit => Items?.Where(x => x.Price > 0)?.ToList()?.Sum(item => item.Price) ?? 0;

        public List<IMarketItem> Items { get; set; }

        public List<Description> Description { get; set; }

        public BitmapImage Icon
        {
            get => icon;
            set
            {
                icon = value;

                RaisePropertyChanged("OnIconLoaded");
            }
        }


        public LootBoxBase()
        {
            Items = new List<IMarketItem>();
            Description = new List<Description>();
        }

        public LootBoxBase(string name)
        {
            Items = new List<IMarketItem>();
            Description = new List<Description>();

            Name = name;
        }


        public void AddItem(IMarketItem item)
        {
            var marketItem = item as MarketItem;

            if (marketItem != null)
            {
                marketItem.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "OnPriceLoaded")
                        RaisePropertyChanged("OnPriceLoaded");
                };
            }

            Items.Add(item);

            RaisePropertyChanged("Items");
        }

        public virtual void GetIcon(string code, string name)
        {
            RequestConveyorSingleton.GetInstance().AddAction(RequestConveyorSingleton.Priority.Icon, () =>
            {
                Icon = RequestsUtil.GetImageLootBox(code, name);

                if (Icon == null)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }

        public abstract void LoadItems();

        public virtual void LoadDescription()
        {
            RequestConveyorSingleton.GetInstance().AddAction(RequestConveyorSingleton.Priority.Description, async () =>
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

                RaisePropertyChanged("OnDescriptionLoaded");

                LoadItems();

                return true;
            });
        }
    }
}
