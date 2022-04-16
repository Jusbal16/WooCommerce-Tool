using Exceptionless.Models.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Helpers;
using WooCommerce_Tool.Settings;

namespace WooCommerce_Tool.ViewsModels
{
    class OrderGenerationViewModel : NotifyPropertyBase
    {
        private OrderGenerationSettings Settings;
        private OrderGenerationConstants Constants;
        private ObservableDictionary<int, string> _MonthSpanComboData;
        private List<string> _MonthComboData;
        private List<string> _TimeComboData;
        private string status;
        public OrderGenerationViewModel(OrderGenerator orderGenerator)
        {
            this.Settings = new OrderGenerationSettings();
            this.Constants = new OrderGenerationConstants();
            _MonthSpanComboData = new ObservableDictionary<int, string>();
            _MonthSpanComboData.Add(0, "Select Month Span");
            for (int i = 1; i <= Constants.MonthSpan; i += 2)
            {
                _MonthSpanComboData.Add(i, i.ToString() + " Month");
                if (i >= Constants.ForecastingPeriod)
                    i++;

            }
            _MonthComboData = new List<string>();
            _MonthComboData.Add("Select Date");
            foreach (var date in Constants.DateConstants)
                _MonthComboData.Add(date);
            _TimeComboData = new List<string>();
            _TimeComboData.Add("Select Time");
            foreach (var time in Constants.TimeConstants)
                _TimeComboData.Add(time);
            orderGenerator.ValueChanged += orderGenerator_ValueChanged;
        }
        // Date binding from ui
        public string Date
        {
            get { return Settings.Date; }
            set
            {
                if (Settings.Date != value)
                {
                    Settings.Date = value;
                    OnPropertyChanged("Date");
                }
            }
        }
        // Time binding from ui
        public string Time
        {
            get { return Settings.Time; }
            set
            {
                if (Settings.Time != value)
                {
                    Settings.Time = value;
                    OnPropertyChanged("Time");
                }
            }
        }
        // OrderCount binding from ui
        public int OrderCount
        {
            get { return Settings.OrderCount; }
            set
            {
                if (Settings.OrderCount != value)
                {
                    Settings.OrderCount = value;
                    OnPropertyChanged("OrderCount");
                }
            }
        }
        // MonthsCount binding from ui
        public int MonthsCount
        {
            get { return Settings.MonthsCount; }
            set
            {
                if (Settings.MonthsCount != value)
                {
                    Settings.MonthsCount = value;
                    OnPropertyChanged("MonthsCount");
                }
            }
        }
        private void orderGenerator_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.Status = e.NewValue;
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
        // MonthSpan item source binding combobox
        public ObservableDictionary<int, string> MonthSpanComboData
        {
            get { return _MonthSpanComboData; }
            set { _MonthSpanComboData = value; }
        }
        // Month item source binding combobox
        public List<string> MonthComboData
        {
            get { return _MonthComboData; }
            set { _MonthComboData = value; }
        }
        // Time item source binding combobox
        public List<string> TimeComboData
        {
            get { return _TimeComboData; }
            set { _TimeComboData = value; }
        }
    }
}
