using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace KFMarketAnalysis.Models.Interfaces
{
    [JsonObject]
    public interface ILootBox: INotifyPropertyChanged
    {
        //event EventHandler OnDescriptionLoading;
        //event EventHandler OnDescriptionLoaded;

        //event EventHandler OnListItemsLoading;
        //event EventHandler OnListItemsLoaded;

        //event EventHandler OnItemLoaded;
        //event EventHandler OnPriceLoaded;

        //event EventHandler OnListPricesLoading;
        //event EventHandler OnListPricesLoaded;

        //event EventHandler OnIconLoading;
        //event EventHandler OnIconLoaded;


        string Name { get; set; }

        double Profit { get; }
        
        /// <summary>
        /// Предметы, которые содержатся в лутбоксе
        /// </summary>
        List<MarketItem> Items { get; set; }
        
        /// <summary>
        /// Описание
        /// </summary>
        List<Description> Description { get; set; }

        /// <summary>
        /// Иконка
        /// </summary>
        BitmapImage Icon { get; set; }


        void Update();

        void LoadItems();

        void AddItem(IMarketItem item);

        void GetIcon(string code, string name);
    }
}
