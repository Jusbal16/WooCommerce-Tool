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
using WooCommerce_Tool.Settings;
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
        private OrderPredictionSettings Settings;
        public OrderPredictionView(Main main)
        {
            this.Main = main;
            _viewModel = new OrderPredictionViewModel();
            Settings = new OrderPredictionSettings();
            DataContext = _viewModel;
            InitializeComponent();
            comboBoxStartDate.SelectedIndex = 0;
            comboBoxEndDate.SelectedIndex = 0;
            comboBoxMonth.SelectedIndex = 0;
            comboBoxTime.SelectedIndex = 0;
        }
        private void Button_Click_Prediction(object sender, RoutedEventArgs e)
        {
            CleanUp();
            if (Main.OrderService.OrdersFlag)
                if (CheckFill())
                {
                    Settings.StartDate = _viewModel.StartDate;
                    Settings.EndDate = _viewModel.EndDate;
                    Settings.Month = _viewModel.Month;
                    Settings.Time = _viewModel.Time;
                    Task.Run(() => Predictions(Settings));
                }
                else
                {
                    ShowMessage("Not all settings are selected", "Error");
                }
            else
                ShowMessage("Still downloading data, please wait", "Error");

        }
        public void Predictions(OrderPredictionSettings settings)
        {
            _viewModel.Status = "Downloading orders";
            Main.PredGetData(settings);
            _viewModel.Status = "Calculating month time probability";
            Main.ProbOrdersMonthTime();
            _viewModel.Status = "Calculating time probability";
            Main.ProbOrdersTime();
            _viewModel.Status = "Forecasting started";
            Main.PredOrderForecasting();
            _viewModel.Status = "Finished";
            Main.FindBestForecastingMethod();
            Main.LinerRegresionWithNeuralNetwork();
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
            if (comboBoxStartDate.SelectedIndex == 0 || comboBoxEndDate.SelectedIndex == 0 || comboBoxMonth.SelectedIndex == 0 || comboBoxTime.SelectedIndex == 0)
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

    }
}
