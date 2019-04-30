using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models
{
    [Serializable]
    [DebuggerDisplay("{Name}")]
    public class MarketItem : BindableBase, IMarketItem
    {
        private double price;
        private BitmapImage icon;


        public string Name { get; set; }

        [JsonIgnore]
        public BitmapImage Icon
        {
            get => icon;
            set
            {
                icon = value;
                RaisePropertyChanged("OnIconLoaded");
            }
        }

        public string IconUri
        {
            get => Icon?.UriSource?.ToString();
            set => Icon = new BitmapImage(new Uri(value));
        }

        public double Price
        {
            get => price;
            set
            {
                price = value;

                RaisePropertyChanged("OnPriceLoaded");
            }
        }


        public MarketItem() { }

        public MarketItem(string name)
        {
            Name = name;
        }


        public void GetIcon(string code, string name, string lootBox = "")
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.WithoutDelay, () =>
            {
                Icon = RequestsUtil.GetImageItem(code, name, lootBox);

                if (Icon == null)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }

        public void GetPrice()
        {
            RequestHandler.GetInstance().AddAction(RequestHandler.Priority.Low, () =>
            {
                Price = RequestsUtil.GetPrice(Name);

                if (Price == -2)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }
    }
}
