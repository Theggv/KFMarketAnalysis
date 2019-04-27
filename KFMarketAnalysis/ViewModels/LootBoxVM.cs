using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using KFMarketAnalysis.Models;
using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
using System.Windows.Media.Imaging;
using Prism.Commands;
using System.Windows.Media;
using KFMarketAnalysis.Models.LootBoxes;

namespace KFMarketAnalysis.ViewModels
{
    public class LootBoxVM : BindableBase
    {
        public ILootBox LootBox { get; private set; }
        
        public string Name => LootBox?.Name;

        public string ProfitWithoutBundle =>
            $"Without bundle:\t{Converters.ConvertToPrice(GetProfitWithoutBundle())}";

        public string ProfitWithBundle =>
             $"With bundle:\t{Converters.ConvertToPrice(GetProfitWithBundle())}";

        public string NumItems =>
            $"Items count:\t{LootBox?.Items?.Count}";

        public string NumLoadedItems =>
            $"Loaded:\t\t{LootBox?.Items?.Where(item => item.Price > 0).Count()}";

        public string NumErrors =>
            $"Errors:\t\t{LootBox?.Items?.Where(item => item.Price < 0).Count()}";

        public Color ProfitWithoutBundleColor
        {
            get
            {
                var profit = GetProfitWithoutBundle();

                if (profit < 0)
                    return Colors.Red;

                if (profit < 1000)
                    return Colors.Yellow;

                return Colors.Green;
            }
        }
        public Color ProfitWithBundleColor
        {
            get
            {
                var profit = GetProfitWithBundle();
                
                if (profit < 0)
                    return Colors.Red;

                if (profit < 1000)
                    return Colors.Yellow;

                return Colors.Green;
            }
        }


        public MarketItemListVM MarketItemListVM { get; set; }


        public BitmapImage Icon => LootBox?.Icon;

        public DelegateCommand Click { get; set; }

        public DelegateCommand Update { get; set; }


        public LootBoxVM(ILootBox lootBox)
        {
            LootBox = lootBox;

            (LootBox as LootBoxBase).PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "OnPriceLoaded")
                {
                    Update.Execute();
                    
                    RaisePropertyChanged("OnItemLoaded");
                }

                if(e.PropertyName == "OnIconLoaded")
                {
                    Update.Execute();
                }

                if (e.PropertyName == "OnItemLoaded")
                {
                    var item = LootBox.Items?.Last();

                    if (item != null)
                        MarketItemListVM.AddLootBox(item);

                    Update.Execute();

                    RaisePropertyChanged("OnItemLoaded");
                }

                if (e.PropertyName == "OnDescriptionLoaded")
                {
                    RaisePropertyChanged("OnDescriptionLoaded");
                }

                if(e.PropertyName == "OnLoadCompleted")
                {
                    // nothing
                }
            };

            MarketItemListVM = new MarketItemListVM(this, LootBox.Items?
                .Select(x => new MarketItemVM(x)));

            Click = new DelegateCommand(() =>
            {

            });

            Update = new DelegateCommand(() =>
            {
                RaisePropertyChanged(nameof(Icon));

                RaisePropertyChanged(nameof(Name));

                RaisePropertyChanged(nameof(ProfitWithBundle));
                RaisePropertyChanged(nameof(ProfitWithBundleColor));

                RaisePropertyChanged(nameof(ProfitWithoutBundle));
                RaisePropertyChanged(nameof(ProfitWithoutBundleColor));

                RaisePropertyChanged(nameof(NumItems));
                RaisePropertyChanged(nameof(NumLoadedItems));
                RaisePropertyChanged(nameof(NumErrors));
            });

            Update.Execute();
        }

        private double GetProfitWithoutBundle()
        {
            return LootBox?.Profit * 0.87 - 170 * LootBox?.Items.Count ?? 0;
        }

        private double GetProfitWithBundle()
        {
            return LootBox?.Profit * 0.87 - 102.5 * LootBox?.Items.Count ?? 0;
        }
    }
}
