using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WooCommerce_Tool.DB_Models;
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool.Views
{
    /// <summary>
    /// Interaction logic for StorePredictions.xaml
    /// </summary>
    public partial class StorePredictionsView : UserControl
    {
        public Main main { get; set; }
        private StorePredictionsViewModel _viewModel;
        public StorePredictionsView(Main main)
        {
            this.main = main;
            _viewModel = new StorePredictionsViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            comboBoxType.SelectedIndex = 0;
            RefreshNameList();
        }
        // insert selected data to db
        private void Button_Click_Save(object sender, RoutedEventArgs e)
        {
            if (CheckFill())
            {
                if (CheckName())
                {
                    Task.Run(() => AddToDB());
                }
                else
                {
                    ShowMessage("Name already exits in data base", "Error");
                }
            }
            else
            {
                ShowMessage("Not all settings are selected", "Error");
            }
        }
        // delete selected data from db
        private void Button_Click_Delete(object sender, RoutedEventArgs e)
        {
            if (DeletePrediction.SelectedIndex != 0)
            {
                string name = _viewModel.DeletionName;
                main.Delete(name);
                RefreshNameList();
                _viewModel.Status = "Prediction(" + name + ") deleted ";
            }
            else
            {
                ShowMessage("Not all settings are selected", "Error");
            }
        }
        //Add to db selected data
        private void AddToDB()
        {
            ToolOrder order = new ToolOrder();
            ToolProduct product = new ToolProduct();

            if (_viewModel.Type == "Only orders" || _viewModel.Type == "Both")
            {
                _viewModel.Status = "Adding order prediction to data base";
                order = _viewModel.ReturnOrderObject(main.OrderPrediction.Settings);
                main.AddToDB(order, null);
            }
            if (_viewModel.Type == "Only products" || _viewModel.Type == "Both")
            {
                _viewModel.Status = "Adding product prediction to data base";
                product = _viewModel.ReturnProductObject(main.ProductPrediction.Settings);
                main.AddToDB(null, product);
            }
            RefreshNameList();
            _viewModel.Status = "Successfully added";

        }
        // check if all forms ar filled
        public bool CheckFill()
        {
            if (String.IsNullOrEmpty(Name.Text))
                return false;
            if (comboBoxType.SelectedIndex == 0)
                return false;
            return true;
        }
        // check what data to insert in db
        public bool CheckName()
        {
            string name = _viewModel.Name;
            if (_viewModel.Type == "Only orders" && main.ReturnSavedPredictionsNamesOnlyOrders().Contains(name))
                return false;
            if (_viewModel.Type == "Only products" && main.ReturnSavedPredictionsNamesOnlyProducts().Contains(name))
                return false;
            if (_viewModel.Type == "Both" && main.ReturnSavedPredictionsNames().Contains(name))
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
        // refresh category combobox names from db
        public void RefreshNameList()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _viewModel.DeletionComboData.Clear();
                _viewModel.DeletionComboData.Add("Select data to delete");
                DeletePrediction.SelectedIndex = 0;
                ObservableCollection<string> listOfName = new ObservableCollection<string>(main.ReturnSavedPredictionsNames());
                foreach (string t in listOfName)
                    _viewModel.DeletionComboData.Add(t);
            });
        }
    }
}
