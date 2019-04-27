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
    public class LootBoxListVM: BindableBase
    {
        private LootBoxListModel model;
        private LootBoxVM selectedLootBox;

        private object _lock = new object();

        public ObservableCollection<LootBoxVM> LootBoxes { get; set; }

        public LootBoxVM SelectedLootBox
        {
            get => selectedLootBox;
            set
            {
                selectedLootBox = value;

                RaisePropertyChanged("LootBoxSelected");
            }
        }

        public LootBoxListVM()
        {
            model = new LootBoxListModel(this);

            LootBoxes = new ObservableCollection<LootBoxVM>();
            BindingOperations.EnableCollectionSynchronization(LootBoxes, _lock);
            
            Task.Run(() => GetLootBoxes());
        }
        public void AddLootBox(ILootBox item)
        {
            var lootBoxVM = new LootBoxVM(item);
            LootBoxes.Add(lootBoxVM);

            lootBoxVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnDescriptionLoaded")
                    RaisePropertyChanged("OnDescriptionLoaded");

                if (e.PropertyName == "OnItemLoaded")
                    RaisePropertyChanged("OnItemLoaded");
            };
        }

        private void GetLootBoxes()
        {
            model.GetLootBoxes();
        }
    }
}
