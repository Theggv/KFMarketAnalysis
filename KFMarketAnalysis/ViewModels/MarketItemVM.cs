using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFMarketAnalysis.Models;
using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Media;

namespace KFMarketAnalysis.ViewModels
{
    public class MarketItemVM: BindableBase
    {
        public IMarketItem Item { get; private set; }

        public string Name => Item?.Name;

        public string Price =>
            $"Price:\t{Converters.ConvertToPrice(Item?.Price, true)}";

        public Color NameColor { get; set; } = Colors.Black;
        

        public BitmapImage Icon => Item?.Icon;

        public DelegateCommand Update { get; set; }

        public DelegateCommand DoubleClick { get; set; }


        public MarketItemVM(IMarketItem item)
        {
            Item = item;

            Item.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnPriceLoaded")
                {
                    RaisePropertyChanged(nameof(Price));

                    RaisePropertyChanged("OnItemLoaded");
                }

                if (e.PropertyName == "OnIconLoaded")
                {
                    RaisePropertyChanged(nameof(Icon));
                }
            };

            DoubleClick = new DelegateCommand(() =>
            {
                RequestsUtil.OpenInBrowser(Item);
            });


            Update = new DelegateCommand(() =>
            {
                RaisePropertyChanged(nameof(Icon));

                RaisePropertyChanged(nameof(Name));
                RaisePropertyChanged(nameof(NameColor));

                RaisePropertyChanged(nameof(Price));
            });

            Update.Execute();
        }
    }
}
