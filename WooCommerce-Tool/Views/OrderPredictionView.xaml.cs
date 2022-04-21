using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
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
using GalaSoft.MvvmLight.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using WooCommerce_Tool.DB_Models;
using WooCommerce_Tool.PredictionModels;
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
            RefreshNameList();
            PredictionChart.Visibility = Visibility.Hidden;
        }
        private void Button_Click_Prediction(object sender, RoutedEventArgs e)
        {
            PredictionChart.Visibility = Visibility.Visible;
            Label3.Visibility = Visibility.Visible;
            Label4.Visibility = Visibility.Visible;
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
            if (!checkData(settings))
            {
                ShowMessage("Not enough data","Error");
                return;
            }
            _viewModel.Status = "Calculating month time probability";
            Main.ProbOrdersMonthTime();
            _viewModel.Status = "Calculating time probability";
            Main.ProbOrdersTime();
            _viewModel.Status = "Forecasting started";
            Main.PredOrderForecasting();
            _viewModel.Status = "Finished";
            Main.FindBestForecastingMethod();
            Main.LinerRegresionWithNeuralNetwork();
            CreateResultText();
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
        // check if all forms ar filled
        public bool CheckFill()
        {
            if (comboBoxStartDate.SelectedIndex == 0 || comboBoxEndDate.SelectedIndex == 0 || comboBoxMonth.SelectedIndex == 0 || comboBoxTime.SelectedIndex == 0)
                return false;
            return true;
        }
        //clean up graph for new data
        public void CleanUp()
        {
            _viewModel.BarLabels = null;
            _viewModel.Status = null;
            _viewModel.OrdersCount.Clear();
            _viewModel.MonthProbability.Clear();
            _viewModel.TimeProbability.Clear();
        }
        // fill category with new data
        public void RefreshNameList()
        {
            _viewModel.NamesComboData.Clear();
            _viewModel.NamesComboData.Add("Select data to delete");
            comboBoxDBNames.SelectedIndex = 0;
            ObservableCollection<string> listOfName = new ObservableCollection<string>(Main.ReturnSavedPredictionsNamesOnlyOrders());
            foreach (string t in listOfName)
                _viewModel.NamesComboData.Add(t);
        }
        // fill graph with selected data from db
        private void comboBoxDBNames_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBoxDBNames.SelectedIndex == 0)
                return;
            PredictionChart.Visibility = Visibility.Visible;
            Label3.Visibility = Visibility.Visible;
            Label4.Visibility = Visibility.Visible;
            //clear graph
            CleanUp();
            string Name = _viewModel.Name;
            ToolOrder Order = Main.ReturnOrderByName(Name);
            //
            comboBoxStartDate.SelectedValue = Order.StartDate;
            comboBoxEndDate.SelectedValue = Order.EndDate;
            comboBoxMonth.SelectedValue = Order.TimeOfTheMonth;
            comboBoxTime.SelectedValue = Order.TimeOfTheDay;
            //
            IEnumerable<OrdersMontlyData> data = JsonSerializer.Deserialize<IEnumerable<OrdersMontlyData>>(Order.TotalOrder);
            Messenger.Default.Send<IEnumerable<OrdersMontlyData>>(data);
            //
            List<OrdersMonthTimeProbability> data1 = JsonSerializer.Deserialize<List<OrdersMonthTimeProbability>>(Order.ProbabilityTimeOfTheMonth);
            Messenger.Default.Send<List<OrdersMonthTimeProbability>>(data1);
            //
            List<OrdersTimeProbability> data2 = JsonSerializer.Deserialize<List<OrdersTimeProbability>>(Order.ProbabilityTimeOfTheDay);
            Messenger.Default.Send<List<OrdersTimeProbability>>(data2);
            //
            NNOrderData data3 = JsonSerializer.Deserialize<NNOrderData>(Order.NnOrder);
            Messenger.Default.Send<NNOrderData>(data3);
            //
            OrdersMontlyForecasting data4 = JsonSerializer.Deserialize<OrdersMontlyForecasting>(Order.TimeSeriesOrder);
            Messenger.Default.Send<OrdersMontlyForecasting>(data4);
            //
            List<MLPredictionDataOrders> data5 = JsonSerializer.Deserialize<List<MLPredictionDataOrders>>(Order.RegresionOrder);
            Messenger.Default.Send<List<MLPredictionDataOrders>>(data5);
            CreateResultText();
        }
        // check data if predictions are possible
        public bool checkData(OrderPredictionSettings settings)
        {
            var StartDate = DateTime.Parse(settings.StartDate);
            var EndDate = DateTime.Parse(settings.EndDate);
            int months = ((EndDate.Year - StartDate.Year) * 12) + EndDate.Month - StartDate.Month;
            int dataCount = Main.OrderPrediction.SortedOrdersData.Count();
            if (months + 1 == dataCount)
            {
                return true;
            }
            if (dataCount > 8)
                return true;
            return false;
        }
        public void CreateResultText()
        {
            // calculate if graph is rising, or decreasing with trendline slope
            int a = Main.CalculateSlope(_viewModel.ForecastedValues);
            int b = Main.CalculateSlope(_viewModel.ForecastedMLValues);
            int c = Main.CalculateSlope(_viewModel.ForecastedNNValues);
            string OrderText = Main.ReturnForecastedResultText(a + b + c);
            double percentage = CalculatePercentage(OrderText);
            // create result text;
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                string text = $"Order prediction results from <b>\"{_viewModel.StartDate}\"<b> to <b>\"{_viewModel.EndDate}\"<b>" +
                     $" with time of the month <b>\"{_viewModel.Month}\"<b> and time of the day <b>\"{_viewModel.Time}\"<b>:\n\n";
                if (OrderText == "stay normal")
                    text += $"\t • In next 3 months orders will <b>{OrderText}<b>.\n";
                else
                    text += $"\t • In next 3 months orders will <b>{OrderText}<b> by <b>{percentage}%<b> from the last three months\n";
                text += $"\t • Most popular time of the month for orders <b>\"{_viewModel.MonthProbability.ElementAt(0).Title}\"<b>.\n" +
                     $"\t • Most popular time of the day for orders <b>\"{_viewModel.TimeProbability.ElementAt(0).Title}\"<b> and " +
                     $"<b>\"{_viewModel.TimeProbability.ElementAt(1).Title}\"<b>.\n";
                //_viewModel.ResultText = text;
                Main.AddTextToTextBlock(text, Results);
            });

        }
        // calculate percentage of fall or rise of order by average order from last three months
        public double CalculatePercentage(string orderResult)
        {
            if (orderResult == "stay normal")
                return 0;
            double a = _viewModel.ForecastedValues[_viewModel.ForecastedValues.Count-1];
            double b = _viewModel.ForecastedMLValues[_viewModel.ForecastedMLValues.Count - 1];
            double c = _viewModel.ForecastedNNValues[_viewModel.ForecastedNNValues.Count - 1];
            double averageForecast = (a + b + c) / 3;
            double aa = _viewModel.TotalOrders[_viewModel.TotalOrders.Count - 1];
            double bb = _viewModel.TotalOrders[_viewModel.TotalOrders.Count - 2];
            double cc = _viewModel.TotalOrders[_viewModel.TotalOrders.Count - 3];
            double averageTotal = (aa + bb + cc) / 3;
            double percentage;
            if (averageForecast > averageTotal)
                percentage = averageTotal / averageForecast;
            else
                percentage = averageForecast / averageTotal;
            percentage = (1 - percentage) * 100; 
            return Math.Round(percentage,2);
        }

    }
}
