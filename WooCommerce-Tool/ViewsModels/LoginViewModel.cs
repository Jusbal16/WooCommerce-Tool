using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Helpers;
using WooCommerce_Tool.Settings;

namespace WooCommerce_Tool.ViewsModels
{
    public class LoginViewModel : NotifyPropertyBase
    {
        private LoginSettings Settings;
        public LoginViewModel()
        {
            Settings = new LoginSettings();
        }
        // URL binding from ui
        public string URL
        {
            get { return Settings.URL; }
            set
            {
                if (Settings.URL != value)
                {
                    Settings.URL = value;
                    OnPropertyChanged("URL");
                }
            }
        }
        // Key binding from ui
        public string Key
        {
            get { return Settings.Key; }
            set
            {
                if (Settings.Key != value)
                {
                    Settings.Key = value;
                    OnPropertyChanged("Key");
                }
            }
        }
        // Secret binding from ui
        public string Secret
        {
            get { return Settings.Secret; }
            set
            {
                if (Settings.Secret != value)
                {
                    Settings.Secret = value;
                    OnPropertyChanged("Secret");
                }
            }
        }
    }
}
