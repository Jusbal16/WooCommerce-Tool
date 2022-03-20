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
using LiveCharts;
using LiveCharts.Wpf;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool.Views
{
    /// <summary>
    /// Interaction logic for OrderPredictionView.xaml
    /// </summary>
    public partial class OrderPredictionView : UserControl
    {
        private Main Main { get; set; }
        private OrderPredictionViewModel _viewModel;
        public OrderPredictionView(Main main)
        {
            this.Main = main;
            _viewModel = new OrderPredictionViewModel();
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
            Main.PredGetData();
            _viewModel.Status = "Calculating month time probability";
            Main.ProbOrdersMonthTime();
            _viewModel.Status = "Calculating time probability";
            Main.ProbOrdersTime();
            _viewModel.Status = "Forecasting started";
            Main.PredOrderForecasting();
            _viewModel.Status = "Finished";
        }
    }
}
