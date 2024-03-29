﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Helpers;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Defaults;
using GalaSoft.MvvmLight.Messaging;
using System.Windows;
using WooCommerce_Tool.PredictionModels;
using WooCommerce_Tool.Settings;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WooCommerce_Tool.ViewsModels
{
    public class ProductPredictionViewModel : NotifyPropertyBase
    {
        private Func<double, string> _BarFormatter;
        private string[] _BarLabels;
        private SeriesCollection _OrderCount;
        private SeriesCollection _ProductProbability;
        private SeriesCollection _CategoryProbability;
        private string status;
        public ChartValues<double> ForecastedValues;
        public ChartValues<double> ForecastedMLValues;
        public ChartValues<double> ForecastedNNValues;
        public ChartValues<double> TotalOrders;
        private List<string> _StartDateComboData;
        private List<string> _EndDateComboData;
        private ObservableCollection<string> _NamesComboData;
        private ObservableCollection<string> _CategoryComboData;
        private ProductPredictionSettings Settings { get; set; }
        private PredictionConstants Constants { get; set; }
        public ProductPredictionViewModel()
        {
            OrdersCount = new SeriesCollection();
            ProductProbability = new SeriesCollection();
            CategoryProbability = new SeriesCollection();
            Settings = new ProductPredictionSettings();
            Constants = new PredictionConstants();
            Messenger.Default.Register<List<ProductPopularData>>(this, (action) => ReceivePopularProducts(action));
            Messenger.Default.Register<List<ProductCategoriesData>>(this, (action) => ReceiveCategories(action));
            Messenger.Default.Register<IEnumerable<ProductMontlyData>>(this, (action) => ReceiveOrders(action));
            Messenger.Default.Register<ProductMontlyForecasting>(this, (action) => ReceiveForecasting(action));
            Messenger.Default.Register<List<MLPredictionDataProducts>>(this, (action) => ReceiveMLForecasting(action));
            Messenger.Default.Register<NNProductData>(this, (action) => ReceiveNNForecasting(action));
            //filling combobox
            StartDateComboData = new List<string>();
            EndDateComboData = new List<string>();
            CategoryComboData = new ObservableCollection<string>();
            NamesComboData = new ObservableCollection<string>();
            NamesComboData.Add("Select saved prediction");
            StartDateComboData.Add("Select start date");
            EndDateComboData.Add("Select end date");
            CategoryComboData.Add("Select category");
            CategoryComboData.Add("All");
            // fill combobox with dates
            DateTime datetime = DateTime.Today;
            int monthCount = ((DateTime.Today.Year - datetime.AddYears(-5).Year) * 12) + DateTime.Today.Month - datetime.AddYears(-5).Month;
            string date = null;
            datetime = datetime.AddMonths(-1);
            for (int i = 0; i < monthCount; i++)
            {
                date = ReturnValidDateForm(datetime);
                StartDateComboData.Add(date);
                EndDateComboData.Add(date);
                datetime = datetime.AddMonths(-1);
            }
        }
        // receive popular product probability data and fill it to charts
        private void ReceivePopularProducts(List<ProductPopularData> msg)
        {
            if (msg == null)
            {
                ProductProbability.Clear();
                return;
            }
            List<ProductPopularData> list = new List<ProductPopularData>();
            if (msg.Count() > 10)
            {
                list = (from m in msg
                        orderby m.Count descending
                        select m).Take(5).ToList();
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (var m in list)
                    ProductProbability.Add(new PieSeries { Title = m.ProductName, Values = new ChartValues<ObservableValue> { new ObservableValue(m.Count) } });
            });

        }
        // receive category probability data and fill it to charts
        private void ReceiveCategories(List<ProductCategoriesData> msg)
        {
            if (msg == null)
            {
                CategoryProbability.Clear();
                return;
            }
            List<ProductCategoriesData> list = new List<ProductCategoriesData>();
            if (msg.Count() > 10)
            {
                list = (from m in msg
                        orderby m.Count descending
                        select m).Take(5).ToList();
            }

            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (var m in list)
                    CategoryProbability.Add(new PieSeries { Title = m.CategoryName, Values = new ChartValues<ObservableValue> { new ObservableValue(m.Count) } });
            });

        }
        // receive All order data and fill it to charts
        private void ReceiveOrders(IEnumerable<ProductMontlyData> msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            TotalOrders = new ChartValues<double>();
            ForecastedValues = new ChartValues<double>();
            ForecastedMLValues = new ChartValues<double>();
            ForecastedNNValues = new ChartValues<double>();
            BarLabels = new string[msg.Count()+3];
            int i = 0;
            foreach (var m in msg)
            {
                TotalOrders.Add(Math.Round(m.MoneySpend));
                ForecastedValues.Add(double.NaN);
                ForecastedMLValues.Add(double.NaN);
                ForecastedNNValues.Add(double.NaN);
                BarLabels[i] = m.Year + "/" + m.Month;
                i++;
                //BarLabels.Append(m.Year+"/"+m.Month);

            }
            AddMonths(BarLabels[BarLabels.Length - Constants.ForecastingPeriod -1]);
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Orders Count", Values = TotalOrders, Stroke = Constants.OrderCountBrush, Fill = Constants.OrderCountBrushFill });
            });

        }
        // add months to barlabes for graph
        public void AddMonths(string date)
        {
            int count = BarLabels.Length - Constants.ForecastingPeriod;
            DateTime dateTime = DateTime.Parse(date);
            for (int i = 0; i < Constants.ForecastingPeriod; i++)
            {
                dateTime = dateTime.AddMonths(1);
                BarLabels[count] = dateTime.ToString("yyyy") + "/" + dateTime.ToString("MM");
                count++;
            }
        }
        // receive TimeSeries prediction data and fill it to charts
        private void ReceiveForecasting(ProductMontlyForecasting msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = Constants.ForecastingPeriod;
            int index = 0;
            for (int i = 0; i < horizon; i++)
            {
                index = ForecastedValues.Count() - horizon + i;
                ForecastedValues[index] = Math.Round(msg.ForecastedMoney[i]);
            }
            for (int i = horizon; i < msg.ForecastedMoney.Length; i++)
            {
                ForecastedValues.Add(Math.Round(msg.ForecastedMoney[i]));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Forecasted Values", Values = ForecastedValues, Stroke = Constants.TimeSeriesBrush, Fill = Constants.TimeSeriesBrushFill });
            });

        }
        // receive ML (regresion) prediction data and fill it to charts
        private void ReceiveMLForecasting(List<MLPredictionDataProducts> msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = Constants.ForecastingPeriod;
            int index = 0;
            string methodName = msg.ElementAt(0).MethodName;
            for (int i = 0; i < horizon; i++)
            {
                index = ForecastedMLValues.Count() - horizon + i;
                ForecastedMLValues[index] = Math.Round(msg.ElementAt(i).MoneySpend);
            }
            for (int i = horizon; i < msg.Count(); i++)
            {
                ForecastedMLValues.Add(Math.Round(msg.ElementAt(i).MoneySpend));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = methodName, Values = ForecastedMLValues, Stroke = Constants.RegresionBrush, Fill = Constants.RegresionBrushFill });
            });

        }
        // receive neural network prediction data and fill it to charts
        private void ReceiveNNForecasting(NNProductData msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = Constants.ForecastingPeriod;
            int index = 0;
            for (int i = 0; i < horizon; i++)
            {
                index = ForecastedNNValues.Count() - horizon + i;
                ForecastedNNValues[index] = Math.Round(msg.MoneySpend.ElementAt(i));
            }
            for (int i = horizon; i < msg.MoneySpend.Count(); i++)
            {
                ForecastedNNValues.Add(Math.Round(msg.MoneySpend.ElementAt(i)));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = "Forecasted NN Values", Values = ForecastedNNValues, Stroke = Constants.NNBrush, Fill = Constants.NNBrushFill });
            });
        }
        // BarLabels chart binding from ui
        public string[] BarLabels
        {
            get { return _BarLabels; }
            set
            {
                _BarLabels = value;
                OnPropertyChanged("BarLabels");
            }
        }
        // BarFormatter chart binding from ui
        public Func<double, string> BarFormatter
        {
            get { return _BarFormatter; }
            set {
                _BarFormatter = value; 
            }
        }
        // OrdersCount chart binding from ui
        public SeriesCollection OrdersCount
        {
            get { return _OrderCount; }
            set
            {
                _OrderCount = value;
                OnPropertyChanged("OrdersCount");
            }
        }
        // MonthProbability chart binding from ui
        public SeriesCollection ProductProbability
        {
            get { return _ProductProbability; }
            set
            {
                _ProductProbability = value;
                OnPropertyChanged("MonthProbability");
            }
        }
        // TimeProbability chart binding from ui
        public SeriesCollection CategoryProbability
        {
            get { return _CategoryProbability; }
            set
            {
                _CategoryProbability = value;
                OnPropertyChanged("TimeProbability");
            }
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
        // StartDate item source binding combobox
        public List<string> StartDateComboData
        {
            get { return _StartDateComboData; }
            set { _StartDateComboData = value; }
        }
        // EndDate item source binding combobox
        public List<string> EndDateComboData
        {
            get { return _EndDateComboData; }
            set { _EndDateComboData = value; }
        }
        // names item source binding combobox
        public ObservableCollection<string> NamesComboData
        {
            get { return _NamesComboData; }
            set { _NamesComboData = value; }
        }
        // Category item source binding combobox
        public ObservableCollection<string> CategoryComboData
        {
            get { return _CategoryComboData; }
            set 
            { 
                _CategoryComboData = value;
                OnPropertyChanged("CategoryComboData");
            }
        }
        // name binding from ui
        public string Name
        {
            get { return Settings.Name; }
            set
            {
                if (Settings.Name != value)
                {
                    Settings.Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        // Category binding from ui
        public string Category
        {
            get { return Settings.Category; }
            set
            {
                if (Settings.Category != value)
                {
                    Settings.Category = value;
                    OnPropertyChanged("Category");
                }
            }
        }
        // EndDate binding from ui
        public string EndDate
        {
            get { return Settings.EndDate; }
            set
            {
                if (Settings.EndDate != value)
                {
                    Settings.EndDate = value;
                    OnPropertyChanged("EndDate");
                }
            }
        }
        // StartDate binding from ui
        public string StartDate
        {
            get { return Settings.StartDate; }
            set
            {
                if (Settings.StartDate != value)
                {
                    Settings.StartDate = value;
                    OnPropertyChanged("StartDate");
                }
            }
        }
        // return valid date in string form from datetime 
        private string ReturnValidDateForm(DateTime date)
        {
            return date.ToString("yyyy") + "/" + date.ToString("MM");
        }
    }
}
