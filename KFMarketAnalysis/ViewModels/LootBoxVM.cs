﻿using System;
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
        public enum eState
        {
            /// <summary>
            /// Не загружено
            /// </summary>
            NotLoaded = 0,
            /// <summary>
            /// В очереди
            /// </summary>
            Queue,
            /// <summary>
            /// Загружено описание
            /// </summary>
            DescriptionLoaded,
            /// <summary>
            /// Началась загрузка предметов
            /// </summary>
            LoadStarted,
            /// <summary>
            /// Загрузка списка предметов завершена
            /// </summary>
            ItemsLoaded,
            /// <summary>
            /// Загрузка цен завершена
            /// </summary>
            PricesLoaded
        };

        private eState state = eState.NotLoaded;

        public eState State
        {
            get => state;
            set
            {
                state = value;

                RaisePropertyChanged(nameof(BackgroundColor));
                RaisePropertyChanged("OnStateChanged");
            }
        }

        public ILootBox LootBox { get; private set; }
        
        public string Name => LootBox?.Name;

        public string ProfitWithoutBundle =>
            $"Without bundle:\t{Converters.ConvertToPrice(GetProfitWithoutBundle())}";

        public string ProfitWithBundle =>
             $"With bundle:\t{Converters.ConvertToPrice(GetProfitWithBundle())}";

        public string NumItems =>
            $"Items count:\t{LootBox?.Items?.Count}";

        public string NumLoadedItems =>
            $"Loaded:\t\t{LootBox?.Items?.ToArray().Where(item => item.Price > 0).Count()}";

        public string NumErrors =>
            $"Errors:\t\t{LootBox?.Items?.ToArray().Where(item => item.Price < 0).Count()}";

        public string ProfitWithoutBundleColor
        {
            get
            {
                var profit = GetProfitWithoutBundle();

                if (profit < 0)
                    return Application.Current.Resources["RedTextColor"].ToString();

                if (profit < 1000)
                    return Application.Current.Resources["YellowTextColor"].ToString();

                return Application.Current.Resources["GreenTextColor"].ToString();
            }
        }

        public string ProfitWithBundleColor
        {
            get
            {
                var profit = GetProfitWithBundle();
                
                if (profit < 0)
                    return Application.Current.Resources["RedTextColor"].ToString();

                if (profit < 1000)
                    return Application.Current.Resources["YellowTextColor"].ToString();

                return Application.Current.Resources["GreenTextColor"].ToString();
            }
        }


        public string BackgroundColor
        {
            get
            {
                switch (State)
                {
                    case eState.NotLoaded:
                        return Application.Current.Resources["RedTextColor"].ToString();
                    case eState.Queue:
                        return Application.Current.Resources["WhiteTextColor"].ToString();
                    case eState.DescriptionLoaded:
                        return Application.Current.Resources["LightYellowTextColor"].ToString();
                    case eState.LoadStarted:
                        return Application.Current.Resources["YellowTextColor"].ToString();
                    case eState.ItemsLoaded:
                        return Application.Current.Resources["LightGreenTextColor"].ToString();
                    case eState.PricesLoaded:
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

            LootBox = lootBox;

            LootBox.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "OnPriceLoaded")
                {
                    Update.Execute();

                    if (LootBox.Items.Count(x => x.Price == 0) == 0)
                        State = eState.PricesLoaded;

                    RaisePropertyChanged("OnItemLoaded");
                }

                if(e.PropertyName == "OnIconLoaded")
                {
                    Update.Execute();
                }

                if(e.PropertyName == "OnLoadStarted")
                {
                    State = eState.LoadStarted;
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
                    State = eState.DescriptionLoaded;

                    RaisePropertyChanged("OnDescriptionLoaded");
                }

                if(e.PropertyName == "OnAddInQueue")
                {
                    State = eState.Queue;
                }

                if (e.PropertyName == "OnLoadCompleted")
                {
                    State = eState.ItemsLoaded;
                }
            };

            MarketItemListVM = new MarketItemListVM(this, LootBox.Items?
                .Select(x => new MarketItemVM(x)));

            DoubleClick = new DelegateCommand(() =>
            {
                RequestsUtil.OpenInBrowser(LootBox);
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
