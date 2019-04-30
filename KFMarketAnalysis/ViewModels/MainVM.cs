using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using KFMarketAnalysis.Models;
using System.IO;
using System.Xml.Serialization;
using System.Windows;
using KFMarketAnalysis.Models.Interfaces;
using KFMarketAnalysis.Models.Utility;
using Prism.Commands;
using System.Collections.ObjectModel;

namespace KFMarketAnalysis.ViewModels
{
    public class MainVM: BindableBase
    {
        private ILootBox lootBox;

        public ProxyVM ProxyVM { get; set; }

        public LootBoxVM LootBoxVM { get; set; }

        public LootBoxListVM LootBoxListVM { get; set; }

        public MarketItemListVM MarketItemListVM =>
            LootBoxListVM.SelectedLootBox?.MarketItemListVM;

        public List<DescriptionVM> Descriptions => lootBox?.Description?
            .Select(d => new DescriptionVM(d)).ToList();
        
        public DelegateCommand SearchCommand { get; set; }

        public DelegateCommand Exit { get; set; }


        public MainVM()
        {
            CreateTempDirectory();

            ProxyVM = new ProxyVM();
            LootBoxListVM = new LootBoxListVM();

            LootBoxListVM.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "OnLootBoxSelected")
                {
                    if (LootBoxListVM.SelectedLootBox == null)
                        return;

                    Task.Run(async () =>
                    {
                        lootBox = LootBoxListVM.SelectedLootBox.LootBox;

                        LootBoxVM = LootBoxListVM.SelectedLootBox;

                        await Task.Delay(275);

                        RaisePropertyChanged(nameof(LootBoxVM));
                        RaisePropertyChanged(nameof(MarketItemListVM));

                        RaisePropertyChanged(nameof(Descriptions));
                    });
                }

                if(e.PropertyName == "OnItemLoaded")
                {
                    RaisePropertyChanged(nameof(LootBoxVM));
                    RaisePropertyChanged(nameof(MarketItemListVM));
                }

                if(e.PropertyName == "OnDescriptionLoaded")
                    RaisePropertyChanged(nameof(Descriptions));
            };

            SearchCommand = new DelegateCommand(() =>
            {
                if (LootBoxListVM.SelectedLootBox == null)
                    return;

                lootBox.Update();
            });

            Exit = new DelegateCommand(() =>
            {
                lock(new object())
                {
                    LootBoxListVM.Save.Execute();
                }
            });
        }

        private void CreateTempDirectory()
        {
            if (!Directory.Exists("temp"))
                Directory.CreateDirectory("temp");
        }
    }
}
