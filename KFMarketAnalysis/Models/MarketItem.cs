using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
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

        public BitmapImage Icon
        {
            get => icon;
            set
            {
                icon = value;

                RaisePropertyChanged("OnIconLoaded");
            }
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
            RequestConveyorSingleton.GetInstance().AddAction(RequestConveyorSingleton.Priority.Icon, () =>
            {
                Icon = RequestsUtil.GetImageItem(code, name, lootBox);

                if (Icon == null)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }

        public void GetPrice()
        {
            RequestConveyorSingleton.GetInstance().AddAction(RequestConveyorSingleton.Priority.Price, () =>
            {
                Price = RequestsUtil.GetPrice(Name);

                if (Price == -2)
                    return Task.FromResult(false);

                return Task.FromResult(true);
            });
        }
    }
}
