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
using WooCommerce_Tool.PredictionClasses;
using WooCommerce_Tool.Settings;

namespace WooCommerce_Tool.ViewsModels
{
    public class OrderPredictionViewModel : NotifyPropertyBase
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
        private OrderPredictionSettings Settings { get; set; }
        public OrderPredictionViewModel()
        {
            OrdersCount = new SeriesCollection();
            MonthProbability = new SeriesCollection();
            TimeProbability = new SeriesCollection();
            Settings = new OrderPredictionSettings();
            Messenger.Default.Register<List<OrdersMonthTimeProbability>>(this, (action) => ReceiveMonthTime(action));
            Messenger.Default.Register<List<OrdersTimeProbability>>(this, (action) => ReceiveTime(action));
            Messenger.Default.Register<IEnumerable<OrdersMontlyData>>(this, (action) => ReceiveOrders(action));
            Messenger.Default.Register<OrdersMontlyForecasting>(this, (action) => ReceiveForecasting(action));
            Messenger.Default.Register<List<MLPredictionDataOrders>>(this, (action) => ReceiveMLForecasting(action));
            Messenger.Default.Register<NNOrderData>(this, (action) => ReceiveNNForecasting(action));
            //filling combobox
            StartDateComboData = new List<string>();
            EndDateComboData = new List<string>();
            StartDateComboData.Add("Select start date");
            EndDateComboData.Add("Select end date");
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
        private string ReturnValidDateForm(DateTime date)
        {
            return date.ToString("yyyy")+"/"+date.ToString("MM").Replace("0", string.Empty);
        }
        private void ReceiveMonthTime(List<OrdersMonthTimeProbability> msg)
        {

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach (var m in msg)
                    MonthProbability.Add(new PieSeries { Title = m.MonthPeriod, Values = new ChartValues<ObservableValue> { new ObservableValue(m.OrdersCount) } });
            });

        }
        private void ReceiveTime(List<OrdersTimeProbability> msg)
        {
            List<OrdersTimeProbability> list = new();
            if (msg.Count() > 10)
            {
                list = (from m in msg
                        orderby m.OrdersCount descending
                        select m).Take(5).ToList();
            }

            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach (var m in list)
                    TimeProbability.Add(new PieSeries { Title = (m.Time.ToString() + "h-" + (m.Time + 1).ToString() + "h"), Values = new ChartValues<ObservableValue> { new ObservableValue(m.OrdersCount) } });
            });

        }
        private void ReceiveOrders(IEnumerable<OrdersMontlyData> msg)
        {
            ChartValues<double> Values = new ChartValues<double>();
            ForecastedValues = new ChartValues<double>();
            ForecastedMLValues = new ChartValues<double>();
            ForecastedNNValues = new ChartValues<double>();
            BarLabels = new string[msg.Count() + 3];
            int i = 0;
            foreach (var m in msg)
            {
                Values.Add(m.OrdersCount);
                ForecastedValues.Add(double.NaN);
                ForecastedMLValues.Add(double.NaN);
                ForecastedNNValues.Add(double.NaN);
                BarLabels[i] = m.Year + "/" + m.Month;
                i++;
            }
            AddMonths(BarLabels[BarLabels.Length - 4]);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = "Orders Count", Values = Values, DataLabels = true });
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
        private void ReceiveForecasting(OrdersMontlyForecasting msg)
        {
            int horizon = 3;
            int index = 0;
            for (int i = 0; i < 3; i++)
            {
                index = ForecastedValues.Count() - horizon + i;
                ForecastedValues[index] = Math.Round(msg.ForecastedOrders[i]);
            }
            for (int i = 3; i < msg.ForecastedOrders.Length; i++)
            {
                ForecastedValues.Add(Math.Round(msg.ForecastedOrders[i]));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = "Forecasted Values", Values = ForecastedValues, DataLabels = true });
            });

        }
        private void ReceiveNNForecasting(NNOrderData msg)
        {
            int horizon = 3;
            int index = 0;
            for (int i = 0;i < horizon; i++)
            {
                index = ForecastedNNValues.Count() - horizon + i;
                ForecastedNNValues[index] = Math.Round(msg.OrderCount.ElementAt(i));
            }
            for (int i = 3; i < msg.OrderCount.Count(); i++)
            {
                ForecastedNNValues.Add(Math.Round(msg.OrderCount.ElementAt(i)));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = "Forecasted NN Values", Values = ForecastedNNValues, DataLabels = true });
            });
        }
        private void ReceiveMLForecasting(List<MLPredictionDataOrders> msg)
        {
            int horizon = 3;
            int index = 0;
            string methodName = msg.ElementAt(0).MethodName;
            for (int i = 0; i < 3; i++)
            {
                index = ForecastedMLValues.Count() - horizon + i;
                ForecastedMLValues[index] = Math.Round(msg.ElementAt(i).OrderCount);
            }
            for (int i = 3; i < msg.Count(); i++)
            {
                ForecastedMLValues.Add(Math.Round(msg.ElementAt(i).OrderCount));
            }
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                OrdersCount.Add(new LineSeries { Title = methodName, Values = ForecastedMLValues, DataLabels = true });
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
            set { _BarFormatter = value => value.ToString("N"); }
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



    }
}
