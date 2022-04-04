using System;
using System.Collections.Generic;
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
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool.Views
{
    /// <summary>
    /// Interaction logic for ProductPredictionView.xaml
    /// </summary>
    public partial class ProductPredictionView : UserControl
    {
        private Main Main { get; set; }
        private ProductPredictionViewModel _viewModel;
        private ProductPredictionSettings Settings { get; set; }
        public ProductPredictionView(Main main)
        {
            this.Main = main;
            _viewModel = new ProductPredictionViewModel();
            Settings = new ProductPredictionSettings();
            DataContext = _viewModel;
            InitializeComponent();
            comboBoxStartDate.SelectedIndex = 0;
            comboBoxEndDate.SelectedIndex = 0;
            comboBoxCategory.SelectedIndex = 0;
        }
        private void Button_Click_Prediction(object sender, RoutedEventArgs e)
        {
            CleanUp();
            if (Main.OrderService.OrdersFlag && Main.ProductsService.ProductFlag)
                if (CheckFill())
                {
                    Settings.StartDate = _viewModel.StartDate;
                    Settings.EndDate = _viewModel.EndDate;
                    Settings.Category = _viewModel.Category;    
                    Task.Run(() => Predictions(Settings));
                }
                else
                {
                    ShowMessage("Not all settings are selected", "Error");
                }
            else
                ShowMessage("Still downloading data, please wait", "Error");
        }
        public void Predictions(ProductPredictionSettings settings)
        {
            _viewModel.Status = "Downloading orders";
            Main.PredGetDataProducts(settings);
            _viewModel.Status = "Getting Categories";
            Main.PredGetProductCategories();
            _viewModel.Status = "Getting Products";
            Main.PredGetPopularProducts();
            _viewModel.Status = "Forecasting started";
            Main.PredProductForecasting();
            _viewModel.Status = "Finished";
            Main.FindBestForecastingMethodProduct();
            Main.LinerRegresionWithNeuralNetworkProduct();
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
        public bool CheckFill()
        {
            if (comboBoxStartDate.SelectedIndex == 0 || comboBoxEndDate.SelectedIndex == 0 || comboBoxCategory.SelectedIndex == 0)
                return false;
            return true;
        }
        public void CleanUp()
        {
            _viewModel.BarLabels = null;
            _viewModel.Status = null;
            _viewModel.OrdersCount.Clear();
            _viewModel.MonthProbability.Clear();
            _viewModel.TimeProbability.Clear();
        }
        public void FillCattegeoryComboBox(List<string> cat)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                _viewModel.CategoryComboData.AddRange(cat);
            });

        }
              
    }
}
