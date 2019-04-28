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
        private List<LootBoxVM> lootBoxes;

        private object _lock = new object();

        public IEnumerable<LootBoxVM> LootBoxes
        {
            get => lootBoxes.ToArray().OrderByDescending(x => x.State);
        }

        public LootBoxVM SelectedLootBox
        {
            get => selectedLootBox;
            set
            {
                selectedLootBox = value;

                RaisePropertyChanged("OnLootBoxSelected");
            }
        }

        public LootBoxListVM()
        {
            model = new LootBoxListModel(this);

            lootBoxes = new List<LootBoxVM>();
            BindingOperations.EnableCollectionSynchronization(LootBoxes, _lock);
            
            Task.Run(() => GetLootBoxes());
        }
        public void AddLootBox(ILootBox item)
        {
            var lootBoxVM = new LootBoxVM(item);
            lootBoxes.Add(lootBoxVM);

            RaisePropertyChanged(nameof(LootBoxes));

            lootBoxVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnDescriptionLoaded")
                    RaisePropertyChanged("OnDescriptionLoaded");

                if (e.PropertyName == "OnItemLoaded")
                    RaisePropertyChanged("OnItemLoaded");

                if(e.PropertyName == "OnStateChanged")
                    RaisePropertyChanged(nameof(LootBoxes));
            };
        }

        private void GetLootBoxes()
        {
            model.GetLootBoxes();
        }
    }
}
