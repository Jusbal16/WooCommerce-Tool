using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WooCommerce_Tool.DB_Models;
using WooCommerce_Tool.ViewsModels;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool.Views
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private LoginViewModel _viewModel;
        private MainWindow mainWindow;
        private tool_dbContext _dbContext;
        public Login()
        {
            _dbContext = new tool_dbContext();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            // if setting is set check
            //CheckDB();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (CheckFill())
            {
                string url = _viewModel.URL;
                string key = _viewModel.Key;
                string secret = _viewModel.Secret;
                if (CheckShop(url, key, secret))
                {
                    int id = CheckIfLoginExist(url);
                    if (id == 0)
                    {
                        id++;
                        AddToDB(id, url, key, secret);
                    }
                    if ((bool)RememberMe.IsChecked)
                        setSetting("User", id.ToString());
                    else
                        setSetting("User", null);
                    mainWindow = new MainWindow(id, url, key, secret);
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    ShowMessage("Failed to connect. Please check URL,Key,Secret", "Error");
                }
            }
            else
            {
                ShowMessage("Not all login data is filled", "Error");
            }
            
        }
        private void CheckDB()
        {
            try
            {
                int id = int.Parse(ConfigurationManager.AppSettings["User"]);
                ToolLogin login = ReturnLoginByID(id);
                SetUI(login);

            }
            catch (Exception ex)
            {
                return;
            }

        }
        public bool CheckFill()
        {
            if (String.IsNullOrEmpty(URL.Text) || String.IsNullOrEmpty(Key.Password) || String.IsNullOrEmpty(Secret.Password))
                return false;
            return true;
        }
        public void ShowMessage(string text, string type)
        {
            MessageBoxResult result = MessageBox.Show(text,
                                              type,
                                              MessageBoxButton.OK,
                                              MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        public bool CheckShop(string url, string key, string secret)
        {
            try 
            {
                RestAPI rest = new RestAPI(url, key, secret);
                WCObject wc = new WCObject(rest);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public int CheckIfLoginExist(string url)
        {
            var login = _dbContext.ToolLogins.Where(x => x.Url == url).FirstOrDefault<ToolLogin>();
            if (login == null)
            {
                return 0;
            }
            return (int)login.Id;
        }
        public void AddToDB(int id, string url, string key, string secret)
        {
            ToolLogin login = new ToolLogin();
            login.Id = id;
            login.Url = url;
            login.ApiSecret = secret;
            login.ApiKey = key;
            _dbContext.ToolLogins.Add(login);
            _dbContext.SaveChanges();
        }
        public bool setSetting(string pstrKey, string pstrValue)
        {
            Configuration objConfigFile =
                ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            bool blnKeyExists = false;

            foreach (string strKey in objConfigFile.AppSettings.Settings.AllKeys)
            {
                if (strKey == pstrKey)
                {
                    blnKeyExists = true;
                    objConfigFile.AppSettings.Settings[pstrKey].Value = pstrValue;
                    break;
                }
            }
            if (!blnKeyExists)
            {
                objConfigFile.AppSettings.Settings.Add(pstrKey, pstrValue);
            }
            objConfigFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
            return true;
        }
        public ToolLogin ReturnLoginByID(int key)
        {
            return _dbContext.ToolLogins.Where(x => x.Id == key).FirstOrDefault<ToolLogin>();
        }
        public void SetUI(ToolLogin login)
        {
            URL.Text = login.Url;
            Key.Password = login.ApiKey;
            Secret.Password = login.ApiSecret;
            RememberMe.IsChecked = true;
        }

        private void Key_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Key = Key.Password;
        }

        private void Secret_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Secret = Secret.Password;
        }
    }
}
