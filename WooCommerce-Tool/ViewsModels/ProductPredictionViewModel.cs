using System;
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
using Order_Generation.PredictionTimeSeries;
using WooCommerce_Tool.PredictionClasses;
using WooCommerce_Tool.Settings;
using System.Collections.ObjectModel;

namespace WooCommerce_Tool.ViewsModels
{
    public class ProductPredictionViewModel : NotifyPropertyBase
    {
        private Func<double, string> _BarFormatter;
        private string[] _BarLabels;
        private SeriesCollection _OrderCount;
        private SeriesCollection _MonthProbability;
        private SeriesCollection _TimeProbability;
        private string status;
        ChartValues<double> ForecastedValues;
        ChartValues<double> ForecastedMLValues;
        ChartValues<double> ForecastedNNValues;
        private List<string> _StartDateComboData;
        private List<string> _EndDateComboData;
        private ObservableCollection<string> _NamesComboData;
        private ObservableCollection<string> _CategoryComboData;
        private ProductPredictionSettings Settings { get; set; }
        public ProductPredictionViewModel()
        {
            OrdersCount = new SeriesCollection();
            MonthProbability = new SeriesCollection();
            TimeProbability = new SeriesCollection();
            Settings = new ProductPredictionSettings();
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
        private void ReceivePopularProducts(List<ProductPopularData> msg)
        {
            if (msg == null)
            {
                MonthProbability.Clear();
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
                    MonthProbability.Add(new PieSeries { Title = m.ProductName, Values = new ChartValues<ObservableValue> { new ObservableValue(m.Count) } });
            });

        }
        private void ReceiveCategories(List<ProductCategoriesData> msg)
        {
            if (msg == null)
            {
                TimeProbability.Clear();
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
                    TimeProbability.Add(new PieSeries { Title = m.CategoryName, Values = new ChartValues<ObservableValue> { new ObservableValue(m.Count) } });
            });

        }
        private void ReceiveOrders(IEnumerable<ProductMontlyData> msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            ChartValues<double> Values = new ChartValues<double>();
            ForecastedValues = new ChartValues<double>();
            ForecastedMLValues = new ChartValues<double>();
            ForecastedNNValues = new ChartValues<double>();
            BarLabels = new string[msg.Count()+3];
            int i = 0;
            foreach (var m in msg)
            {
                Values.Add(Math.Round(m.MoneySpend));
                ForecastedValues.Add(double.NaN);
                ForecastedMLValues.Add(double.NaN);
                ForecastedNNValues.Add(double.NaN);
                BarLabels[i] = m.Year + "/" + m.Month;
                i++;
                //BarLabels.Append(m.Year+"/"+m.Month);

            }
            AddMonths(BarLabels[BarLabels.Length - 4]);
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Orders Count", Values = Values });
            });

        }
        public void AddMonths(string date)
        {
            int count = BarLabels.Length - 3;
            DateTime dateTime = DateTime.Parse(date);
            for (int i = 0; i < 3; i++)
            {
                dateTime = dateTime.AddMonths(1);
                BarLabels[count] = dateTime.ToString("yyyy") + "/" + dateTime.ToString("MM");
                count++;
            }
        }
        private void ReceiveForecasting(ProductMontlyForecasting msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = 3;
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                index = ForecastedValues.Count() - horizon + i;
                ForecastedValues[index] = Math.Round(msg.ForecastedMoney[i]);
            }
            for (int i = 3; i < msg.ForecastedMoney.Length; i++)
            {
                ForecastedValues.Add(Math.Round(msg.ForecastedMoney[i]));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Forecasted Values", Values = ForecastedValues });
            });

        }
        private void ReceiveMLForecasting(List<MLPredictionDataProducts> msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = 3;
            int index = 0;
            string methodName = msg.ElementAt(0).MethodName;
            for (int i = 0; i < 3; i++)
            {
                index = ForecastedMLValues.Count() - horizon + i;
                ForecastedMLValues[index] = Math.Round(msg.ElementAt(i).MoneySpend);
            }
            for (int i = 3; i < msg.Count(); i++)
            {
                ForecastedMLValues.Add(Math.Round(msg.ElementAt(i).MoneySpend));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = methodName, Values = ForecastedMLValues });
            });

        }
        private void ReceiveNNForecasting(NNProductData msg)
        {
            if (msg == null)
            {
                OrdersCount.Clear();
                return;
            }
            int horizon = 3;
            int index = 0;
            for (int i = 0; i < horizon; i++)
            {
                index = ForecastedNNValues.Count() - horizon + i;
                ForecastedNNValues[index] = Math.Round(msg.MoneySpend.ElementAt(i));
            }
            for (int i = 3; i < msg.MoneySpend.Count(); i++)
            {
                ForecastedNNValues.Add(Math.Round(msg.MoneySpend.ElementAt(i)));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = "Forecasted NN Values", Values = ForecastedNNValues });
            });
        }
        public string[] BarLabels
        {
            get { return _BarLabels; }
            set
            {
                _BarLabels = value;
                OnPropertyChanged("BarLabels");
            }
        }
        public Func<double, string> BarFormatter
        {
            get { return _BarFormatter; }
            set {
                _BarFormatter = value; 
            }
        }
        public SeriesCollection OrdersCount
        {
            get { return _OrderCount; }
            set
            {
                _OrderCount = value;
                OnPropertyChanged("OrdersCount");
            }
        }
        public SeriesCollection MonthProbability
        {
            get { return _MonthProbability; }
            set
            {
                _MonthProbability = value;
                OnPropertyChanged("MonthProbability");
            }
        }
        public SeriesCollection TimeProbability
        {
            get { return _TimeProbability; }
            set
            {
                _TimeProbability = value;
                OnPropertyChanged("TimeProbability");
            }
        }
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged("Status");
            }
        }
        public List<string> StartDateComboData
        {
            get { return _StartDateComboData; }
            set { _StartDateComboData = value; }
        }
        public List<string> EndDateComboData
        {
            get { return _EndDateComboData; }
            set { _EndDateComboData = value; }
        }
        public ObservableCollection<string> NamesComboData
        {
            get { return _NamesComboData; }
            set { _NamesComboData = value; }
        }
        public ObservableCollection<string> CategoryComboData
        {
            get { return _CategoryComboData; }
            set 
            { 
                _CategoryComboData = value;
                OnPropertyChanged("CategoryComboData");
            }
        }
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
        private string ReturnValidDateForm(DateTime date)
        {
            return date.ToString("yyyy") + "/" + date.ToString("MM").Replace("0", string.Empty);
        }




    }
}
