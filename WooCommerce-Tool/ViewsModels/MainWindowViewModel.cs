using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Helpers;

namespace WooCommerce_Tool.ViewsModels
{
    internal class MainWindowViewModel : NotifyPropertyBase
    {
        private string status;
        public MainWindowViewModel()
        {

        }
        // Status binding from ui
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }

    }
}
