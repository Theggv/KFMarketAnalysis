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
using System.Windows;

namespace KFMarketAnalysis.ViewModels
{
    public class LootBoxVM : BindableBase
    {
        public ILootBox LootBox { get; private set; }

        
        public string Name => LootBox?.Name;


        public string ProfitWithoutBundle =>
            $"Without bundle:\t{Converters.ConvertToPrice(LootBox?.ProfitWithoutBundle)}";

        public string ProfitWithBundle =>
             $"With bundle:\t{Converters.ConvertToPrice(LootBox?.ProfitWithBundle)}";


        public string NumItems =>
            $"Items count:\t{LootBox?.Count}";

        public string NumLoadedItems =>
            $"Loaded:\t\t{LootBox?.Items?.ToArray()?.Count(item => item.Price > 0)}";

        public string NumErrors =>
            $"Errors:\t\t{LootBox?.Items?.ToArray()?.Count(item => item.Price < 0)}";


        public string ProfitWithoutBundleColor
        {
            get
            {
                if (LootBox.ProfitWithoutBundle < 0)
                    return Application.Current.Resources["RedTextColor"].ToString();

                if (LootBox.ProfitWithoutBundle < 1000)
                    return Application.Current.Resources["YellowTextColor"].ToString();

                return Application.Current.Resources["GreenTextColor"].ToString();
            }
        }

        public string ProfitWithBundleColor
        {
            get
            {
                if (LootBox.ProfitWithBundle < 0)
                    return Application.Current.Resources["RedTextColor"].ToString();

                if (LootBox.ProfitWithBundle < 1000)
                    return Application.Current.Resources["YellowTextColor"].ToString();

                return Application.Current.Resources["GreenTextColor"].ToString();
            }
        }


        public string BackgroundColor
        {
            get
            {
                switch (LootBox.State)
                {
                    case LootBoxState.NotLoaded:
                        return Application.Current.Resources["RedTextColor"].ToString();
                    case LootBoxState.Queue:
                        return Application.Current.Resources["WhiteTextColor"].ToString();
                    case LootBoxState.DescriptionLoaded:
                        return Application.Current.Resources["LightYellowTextColor"].ToString();
                    case LootBoxState.LoadStarted:
                        return Application.Current.Resources["YellowTextColor"].ToString();
                    case LootBoxState.ItemsLoaded:
                        return Application.Current.Resources["LightGreenTextColor"].ToString();
                    case LootBoxState.PricesLoaded:
                        return Application.Current.Resources["GreenTextColor"].ToString();
                    default:
                        return Application.Current.Resources["RedTextColor"].ToString();
                }
            }
        }

        public MarketItemListVM MarketItemListVM { get; set; }


        public BitmapImage Icon => LootBox?.Icon;

        public DelegateCommand DoubleClick { get; set; }

        public DelegateCommand Update { get; set; }


        public LootBoxVM(ILootBox lootBox)
        {
            #region Commands init
            Update = new DelegateCommand(() =>
            {
                RaisePropertyChanged(nameof(Icon));

                RaisePropertyChanged(nameof(Name));

                RaisePropertyChanged(nameof(ProfitWithBundle));
                RaisePropertyChanged(nameof(ProfitWithBundleColor));

                RaisePropertyChanged(nameof(ProfitWithoutBundle));
                RaisePropertyChanged(nameof(ProfitWithoutBundleColor));

                RaisePropertyChanged(nameof(BackgroundColor));

                RaisePropertyChanged(nameof(NumItems));
                RaisePropertyChanged(nameof(NumLoadedItems));
                RaisePropertyChanged(nameof(NumErrors));
            });

            DoubleClick = new DelegateCommand(() =>
            {
                RequestsUtil.OpenInBrowser(LootBox);
            });
            #endregion

            LootBox = lootBox;

            LootBox.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == nameof(ILootBox.State))
                {
                    switch (LootBox.State)
                    {
                        case LootBoxState.NotLoaded:
                            break;
                        case LootBoxState.Queue:
                            break;
                        case LootBoxState.DescriptionLoaded:
                            RaisePropertyChanged("OnDescriptionLoaded");
                            break;
                        case LootBoxState.LoadStarted:
                            break;
                        case LootBoxState.ItemsLoaded:
                            break;
                        case LootBoxState.PricesLoaded:
                            break;
                    }

                    RaisePropertyChanged(nameof(ILootBox.State));
                    RaisePropertyChanged(nameof(BackgroundColor));
                }

                if(e.PropertyName == "OnItemLoaded")
                {
                    var item = LootBox.Items?.Last();

                    if (item != null)
                        MarketItemListVM.AddLootBox(item);

                    Update.Execute();

                    RaisePropertyChanged("OnItemLoaded");
                }

                if(e.PropertyName == "OnPriceLoaded")
                {
                    if (LootBox.Count > 0 && LootBox.Items.ToArray().Count(x => x.Price == 0) == 0)
                    {
                        if(LootBox.State != LootBoxState.PricesLoaded)
                            LootBox.State = LootBoxState.PricesLoaded;
                    }

                    Update.Execute();
                }

                if (e.PropertyName == "OnIconLoaded")
                {
                    RaisePropertyChanged(nameof(Icon));
                }
            };


            MarketItemListVM = new MarketItemListVM(this, LootBox.Items?
                .Select(x => new MarketItemVM(x)));

            Update.Execute();

            if(LootBox.State >= LootBoxState.DescriptionLoaded)
                RaisePropertyChanged("OnDescriptionLoaded");
        }
    }
}
