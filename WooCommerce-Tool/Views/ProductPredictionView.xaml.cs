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
        public ProductPredictionView(Main main)
        {
            this.Main = main;
            _viewModel = new ProductPredictionViewModel();
            DataContext = _viewModel;
            InitializeComponent();
     
        }
        private void Button_Click_Prediction(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Predictions());
        }
        public void Predictions()
        {
            _viewModel.Status = "Downloading orders";
            Main.PredGetDataProducts();
            _viewModel.Status = "Getting Categories";
            Main.PredGetProductCategories();
            _viewModel.Status = "Getting Products";
            Main.PredGetPopularProducts();
            _viewModel.Status = "Forecasting started";
            Main.PredProductForecasting();
            _viewModel.Status = "Finished";
            Main.FindBestForecastingMethodProduct();
        }
    }
}
