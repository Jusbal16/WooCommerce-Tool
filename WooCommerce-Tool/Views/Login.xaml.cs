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
            CheckDB();
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
        // check db if shop exist becasue remember me was selected last session
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
        // check if all forms ar filled
        public bool CheckFill()
        {
            if (String.IsNullOrEmpty(URL.Text) || String.IsNullOrEmpty(Key.Password) || String.IsNullOrEmpty(Secret.Password))
                return false;
            return true;
        }
        // show message method
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
        // check if given url,key,secret key is correct by trying to acces shop
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
        // check db if shop exist, and return id
        public int CheckIfLoginExist(string url)
        {
            var login = _dbContext.ToolLogins.Where(x => x.Url == url).FirstOrDefault<ToolLogin>();
            if (login == null)
            {
                return 0;
            }
            return (int)login.Id;
        }
        // Add to DB
        public void AddToDB(int id, string url, string key, string secret)
        {
            ToolLogin login = new ToolLogin();
            login.Id = id;
            login.Url = url;
            login.ApiSecret = Base64Encode(secret);
            login.ApiKey = Base64Encode(key);
            _dbContext.ToolLogins.Add(login);
            _dbContext.SaveChanges();
        }
        // set remember me
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
        // return object from db by id
        public ToolLogin ReturnLoginByID(int key)
        {
            return _dbContext.ToolLogins.Where(x => x.Id == key).FirstOrDefault<ToolLogin>();
        }
        // fill forms if ebject exist in db
        public void SetUI(ToolLogin login)
        {
            URL.Text = login.Url;
            Key.Password = Base64Decode(login.ApiKey);
            Secret.Password = Base64Decode(login.ApiSecret);
            RememberMe.IsChecked = true;
        }
        // password box binding
        private void Key_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Key = Key.Password;
        }
        // password box binding
        private void Secret_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Secret = Secret.Password;
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
