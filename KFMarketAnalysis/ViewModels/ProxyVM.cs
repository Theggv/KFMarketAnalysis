using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KFMarketAnalysis.Models;
using Prism.Commands;
using Prism.Mvvm;

namespace KFMarketAnalysis.ViewModels
{
    public class ProxyVM : BindableBase
    {
        public ProxySingleton Proxy { get; set; }

        public bool IsUse
        {
            get => Proxy.CanUse;
            set
            {
                Proxy.CanUse = value;

                RaisePropertyChanged(nameof(IsUse));
            }
        }

        public string DisplayString => Proxy.IsTesting ? TestProxy : Available;

        public string Available => $"Available count:\t{Proxy.Count}";

        public string TestProxy => $"Test status: {Proxy.TestedCount}/{Proxy.Count}";

        public ProxyVM()
        {
            Proxy = ProxySingleton.GetInstance();

            Proxy.OnTestStarted += (s, e) =>
            {
                RaisePropertyChanged(nameof(DisplayString));
            };

            Proxy.OnProxyTested += (s, e) =>
            {
                RaisePropertyChanged(nameof(DisplayString));
            };

            Proxy.OnTestCompleted += (s, e) =>
            {
                RaisePropertyChanged(nameof(DisplayString));
            };

            Proxy.OnListLoading += (s, e) =>
            {

            };

            Proxy.OnListLoaded += (s, e) =>
            {
                RaisePropertyChanged(nameof(DisplayString));
            };
        }
    }
}
