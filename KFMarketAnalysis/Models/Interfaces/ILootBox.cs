using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.Interfaces
{
    public interface ILootBox
    {
        string Name { get; set; }

        double Profit { get; }
        
        /// <summary>
        /// Предметы, которые содержатся в лутбоксе
        /// </summary>
        List<IMarketItem> Items { get; set; }
        
        /// <summary>
        /// Описание
        /// </summary>
        List<Description> Description { get; set; }

        /// <summary>
        /// Иконка
        /// </summary>
        BitmapImage Icon { get; set; }


        void LoadDescription();

        void LoadItems();

        void AddItem(IMarketItem item);

        void GetIcon(string code, string name);
    }
}
