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
        public OrderPredictionViewModel()
        {
            OrdersCount = new SeriesCollection();
            MonthProbability = new SeriesCollection();
            TimeProbability = new SeriesCollection();
            Messenger.Default.Register<List<OrdersMonthTimeProbability>>(this, (action) => ReceiveMonthTime(action));
            Messenger.Default.Register<List<OrdersTimeProbability>>(this, (action) => ReceiveTime(action));
            Messenger.Default.Register<IEnumerable<OrdersMontlyData>>(this, (action) => ReceiveOrders(action));
            Messenger.Default.Register<OrdersMontlyForecasting>(this, (action) => ReceiveForecasting(action));
        }
        private void ReceiveMonthTime(List<OrdersMonthTimeProbability> msg)
        {
            
            Application.Current.Dispatcher.Invoke((Action)delegate {
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

            Application.Current.Dispatcher.Invoke((Action)delegate {
                foreach (var m in list)
                    TimeProbability.Add(new PieSeries { Title = (m.Time.ToString()+"h-"+(m.Time + 1).ToString()+"h"), Values = new ChartValues<ObservableValue> { new ObservableValue(m.OrdersCount) } });
            });

        }
        private void ReceiveOrders(IEnumerable<OrdersMontlyData> msg)
        {
            ChartValues<double> Values = new ChartValues<double>();
            ForecastedValues = new ChartValues<double>();
            BarLabels = new string[msg.Count()];
            int i = 0;
            foreach (var m in msg)
            {
                Values.Add(m.OrdersCount);
                ForecastedValues.Add(double.NaN);
                BarLabels[i] = m.Year + "/" + m.Month;
                i++;
                //BarLabels.Append(m.Year+"/"+m.Month);

            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Orders Count", Values = Values, DataLabels = true });
            });

        }
        private void ReceiveForecasting(OrdersMontlyForecasting msg)
        {
            int horizon = 3;
            int index = 0;
            for (int i = 0; i < msg.ForecastedOrders.Length; i++)
            {
                index = ForecastedValues.Count() - horizon + i;
                ForecastedValues[index] = Math.Round(msg.ForecastedOrders[i]);
            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Forecasted Values", Values = ForecastedValues, DataLabels = true });
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




    }
}
