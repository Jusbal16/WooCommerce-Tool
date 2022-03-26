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
using Order_Generation.PredictionTimeSeries;
using WooCommerce_Tool.PredictionClasses;

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
        public ProductPredictionViewModel()
        {
            OrdersCount = new SeriesCollection();
            MonthProbability = new SeriesCollection();
            TimeProbability = new SeriesCollection();
            Messenger.Default.Register<List<ProductPopularData>>(this, (action) => ReceivePopularProducts(action));
            Messenger.Default.Register<List<ProductCategoriesData>>(this, (action) => ReceiveCategories(action));
            Messenger.Default.Register<IEnumerable<ProductMontlyData>>(this, (action) => ReceiveOrders(action));
            Messenger.Default.Register<ProductMontlyForecasting>(this, (action) => ReceiveForecasting(action));
        }
        private void ReceivePopularProducts(List<ProductPopularData> msg)
        {
            List<ProductPopularData> list = new();
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
            List<ProductCategoriesData> list = new();
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
            ChartValues<double> Values = new ChartValues<double>();
            ForecastedValues = new ChartValues<double>();
            BarLabels = new string[msg.Count()];
            int i = 0;
            foreach (var m in msg)
            {
                Values.Add(Math.Round(m.MoneySpend));
                ForecastedValues.Add(double.NaN);
                BarLabels[i] = m.Year + "/" + m.Month;
                i++;
                //BarLabels.Append(m.Year+"/"+m.Month);

            }
            Application.Current.Dispatcher.Invoke((Action)delegate {
                OrdersCount.Add(new LineSeries { Title = "Orders Count", Values = Values, DataLabels = true });
            });

        }
        private void ReceiveForecasting(ProductMontlyForecasting msg)
        {
            int horizon = 3;
            int index = 0;
            for (int i = 0; i < msg.ForecastedMoney.Length; i++)
            {
                index = ForecastedValues.Count() - horizon + i;
                ForecastedValues[index] = Math.Round(msg.ForecastedMoney[i]);
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