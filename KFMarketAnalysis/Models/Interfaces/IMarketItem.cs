using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KFMarketAnalysis.Models.Interfaces
{
    public interface IMarketItem
    {
        string Name { get; set; }

        double Price { get; set; }

        BitmapImage Icon { get; set; }

        void GetPrice();

        void GetIcon(string code, string name, string lootBox = "");
    }
}
