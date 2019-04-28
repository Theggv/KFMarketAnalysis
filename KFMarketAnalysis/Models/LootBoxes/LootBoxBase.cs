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
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.LootBoxes
{
    /// <summary>
    /// Базовый класс, представляющий LootBox.
    /// </summary>
    public abstract class LootBoxBase : BindableBase, ILootBox
    {
        protected BitmapImage icon;

        protected string spanPattern = @"<br><span\sstyle=.{1,2}color:\s(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,45})<.{1,2}span>";
        protected string fontPattern = @"<br><font\scolor=.{1,2}(#[0-9|A-F|a-f]{6}).{1,2}>(.{1,45})<.{1,2}font>";

        public string Name { get; set; }

        public double Profit => Items.ToArray().Where(x => x.Price > 0)?.Sum(item => item.Price) ?? 0;

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


        /// <summary>
        /// Добавление предмета в лутбокс
        /// </summary>
        /// <param name="item"></param>
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

        /// <summary>
        /// Загрузка иконки лутбокса
        /// </summary>
        /// <param name="code"></param>
        /// <param name="name"></param>
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

        /// <summary>
        /// Загрузка описания лутбокса
        /// </summary>
        public virtual void LoadDescription()
        {
            RaisePropertyChanged("OnAddInQueue");

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

                RequestHandler.GetInstance().AddAction(RequestHandler.Priority.High, () =>
                {
                    RaisePropertyChanged("OnDescriptionLoaded");

                    return Task.FromResult(true);
                }, false);

                LoadItems();

                return true;
            });
        }

        /// <summary>
        /// Загрузка предметов лутбокса
        /// </summary>
        public abstract void LoadItems();
    }
}
