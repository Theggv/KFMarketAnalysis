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
    /// <summary>
    /// Состояние лутбокса
    /// </summary>
    public enum LootBoxState
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

    [JsonObject]
    public interface ILootBox: INotifyPropertyChanged
    {
        /// <summary>
        /// Статус лутбокса
        /// </summary>
        LootBoxState State { get; set; }


        /// <summary>
        /// Название лутбокса
        /// </summary>
        string Name { get; set; }


        /// <summary>
        /// Прибыль
        /// </summary>
        double Profit { get; }

        /// <summary>
        /// Прибыль с набором
        /// </summary>
        double ProfitWithBundle { get; }

        /// <summary>
        /// Прибыль без набора
        /// </summary>
        double ProfitWithoutBundle { get; }


        /// <summary>
        /// Количество предметов в лутбоксе
        /// </summary>
        int Count { get; }
        
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

        /// <summary>
        /// URI путь к иконке
        /// </summary>
        string IconUri { get; set; }


        void Update();

        void LoadItems();

        void AddItem(IMarketItem item);

        void GetIcon(string code, string name);
    }
}
