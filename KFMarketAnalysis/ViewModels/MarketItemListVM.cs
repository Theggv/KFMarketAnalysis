using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Commands;
using KFMarketAnalysis.Models;
using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;

namespace KFMarketAnalysis.ViewModels
{
    public class MarketItemListVM: BindableBase
    {
        private object _lock = new object();

        private LootBoxVM lootBoxVM;

        private List<MarketItemVM> items;
        private MarketItemVM selectedMarketItem;

        public MarketItemVM SelectedMarketItem
        {
            get => selectedMarketItem;
            set
            {
                if (value == null)
                    return;

                selectedMarketItem = value;

                if (selectedMarketItem.Item?.Price < 0)
                {
                    selectedMarketItem.Item.GetPrice();
                }
            }
        }

        public List<MarketItemVM> Items => items
            .ToArray()
            .OrderByDescending(item => item.Item?.Price).ToList();


        public MarketItemListVM()
        {
            items = new List<MarketItemVM>();
            BindingOperations.EnableCollectionSynchronization(Items, _lock);

            RaisePropertyChanged(nameof(Items));
        }

        public MarketItemListVM(LootBoxVM lootBoxVM, IEnumerable<MarketItemVM> collection)
        {
            this.lootBoxVM = lootBoxVM;

            items = new List<MarketItemVM>(collection);
            BindingOperations.EnableCollectionSynchronization(Items, _lock);

            foreach (var item in items)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == "OnItemLoaded")
                        RaisePropertyChanged(nameof(Items));
                };
            }

            RaisePropertyChanged(nameof(Items));
        }

        public void AddLootBox(IMarketItem item)
        {
            var marketItemVM = new MarketItemVM(item);
            items.Add(marketItemVM);

            marketItemVM.PropertyChanged += (s, e) =>
            {
                if(e.PropertyName == "OnItemLoaded")
                    RaisePropertyChanged(nameof(Items));
            };

            RaisePropertyChanged(nameof(Items));
        }
    }
}
