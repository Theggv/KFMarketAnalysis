using KFMarketAnalysis.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KFMarketAnalysis.ViewModels
{
    public class DescriptionVM: BindableBase
    {
        private Description description;


        public string Text => description?.Text;

        public Color TextColor => description?.Color ?? Colors.Black;

        public DescriptionVM(Description desc)
        {
            description = desc;
        }
    }
}
