using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.PredictionModels;
using System.Text.Json;
using WooCommerce_Tool.DB_Models;
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.Helpers;
using System.Collections.ObjectModel;

namespace WooCommerce_Tool.ViewsModels
{
    public class StorePredictionsViewModel : NotifyPropertyBase
    {
        private ToolOrder OrderData { get; set; }
        private ToolProduct ProductData { get; set; }
        private string status;
        private ObservableCollection<string> _DeletionComboData;
        private List<string> _TypeComboData;
        private StorePredictionSettings Settings { get; set; }
        public StorePredictionsViewModel()
        {
            Settings = new StorePredictionSettings();
            OrderData = new ToolOrder();
            ProductData = new ToolProduct();
            //order
            Messenger.Default.Register<List<OrdersMonthTimeProbability>>(this, (action) => ReceiveMonthTime(action));
            Messenger.Default.Register<List<OrdersTimeProbability>>(this, (action) => ReceiveTime(action));
            Messenger.Default.Register<IEnumerable<OrdersMontlyData>>(this, (action) => ReceiveOrders(action));
            Messenger.Default.Register<OrdersMontlyForecasting>(this, (action) => ReceiveForecasting(action));
            Messenger.Default.Register<List<MLPredictionDataOrders>>(this, (action) => ReceiveMLForecasting(action));
            Messenger.Default.Register<NNOrderData>(this, (action) => ReceiveNNForecasting(action));
            //product
            Messenger.Default.Register<List<ProductPopularData>>(this, (action) => ReceivePopularProducts(action));
            Messenger.Default.Register<List<ProductCategoriesData>>(this, (action) => ReceiveCategories(action));
            Messenger.Default.Register<IEnumerable<ProductMontlyData>>(this, (action) => ReceiveOrdersP(action));
            Messenger.Default.Register<ProductMontlyForecasting>(this, (action) => ReceiveForecastingP(action));
            Messenger.Default.Register<List<MLPredictionDataProducts>>(this, (action) => ReceiveMLForecastingP(action));
            Messenger.Default.Register<NNProductData>(this, (action) => ReceiveNNForecastingP(action));
            // setting data
            DeletionComboData = new ObservableCollection<string>();
            TypeComboData = new List<string>();
            DeletionComboData.Add("Select data to delete");
            TypeComboData.Add("Select type");
            TypeComboData.Add("Both");
            TypeComboData.Add("Only orders");
            TypeComboData.Add("Only products");
        }
        private void ReceivePopularProducts(List<ProductPopularData> msg)
        {
            ProductData.ProbabilityProducts = JsonSerializer.Serialize(msg);
        }
        private void ReceiveCategories(List<ProductCategoriesData> msg)
        {
            ProductData.ProbabilityCategory = JsonSerializer.Serialize(msg);
        }
        private void ReceiveOrdersP(IEnumerable<ProductMontlyData> msg)
        {
            ProductData.TotalProducts = JsonSerializer.Serialize(msg);
        }
        private void ReceiveForecastingP(ProductMontlyForecasting msg)
        {
            ProductData.TimeSeriesProducts = JsonSerializer.Serialize(msg);
        }
        private void ReceiveMLForecastingP(List<MLPredictionDataProducts> msg)
        {
            ProductData.RegresionProducts = JsonSerializer.Serialize(msg);
        }
        private void ReceiveNNForecastingP(NNProductData msg)
        {
            ProductData.NnProducts = JsonSerializer.Serialize(msg);
        }
        // return Order object for db insertion
        public ToolOrder ReturnOrderObject(OrderPredictionSettings settings)
        {
            OrderPredictionSettings OrderSettings = settings;
            OrderData.Name = Name;
            OrderData.StartDate = OrderSettings.StartDate;
            OrderData.EndDate = OrderSettings.EndDate;
            OrderData.TimeOfTheDay = OrderSettings.Time;
            OrderData.TimeOfTheMonth = OrderSettings.Month;
            return OrderData;
        }
        // return product object for db insertion
        public ToolProduct ReturnProductObject(ProductPredictionSettings settings)
        {
            ProductPredictionSettings ProductSettings = settings;
            ProductData.Name = Name;
            ProductData.StartDate = ProductSettings.StartDate;
            ProductData.EndDate = ProductSettings.EndDate;
            ProductData.Category = ProductSettings.Category;
            return ProductData;
        }
        private void ReceiveMonthTime(List<OrdersMonthTimeProbability> msg)
        {
            OrderData.ProbabilityTimeOfTheMonth = JsonSerializer.Serialize(msg);
        }
        private void ReceiveTime(List<OrdersTimeProbability> msg)
        {
            OrderData.ProbabilityTimeOfTheDay = JsonSerializer.Serialize(msg);
        }
        private void ReceiveOrders(IEnumerable<OrdersMontlyData> msg)
        {
            OrderData.TotalOrder = JsonSerializer.Serialize(msg);
        }
        private void ReceiveForecasting(OrdersMontlyForecasting msg)
        {
            OrderData.TimeSeriesOrder = JsonSerializer.Serialize(msg);
        }
        private void ReceiveMLForecasting(List<MLPredictionDataOrders> msg)
        {
            OrderData.RegresionOrder = JsonSerializer.Serialize(msg);
        }
        private void ReceiveNNForecasting(NNOrderData msg)
        {
            OrderData.NnOrder = JsonSerializer.Serialize(msg);
        }
        // Deletion anme item source binding combobox
        public ObservableCollection<string> DeletionComboData
        {
            get { return _DeletionComboData; }
            set { 
                _DeletionComboData = value;
                OnPropertyChanged("DeletionComboData");
            }
        }
        // Type item source binding combobox
        public List<string> TypeComboData
        {
            get { return _TypeComboData; }
            set { _TypeComboData = value; }
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
        // Type binding from ui
        public string Type
        {
            get { return Settings.Type; }
            set
            {
                if (Settings.Type != value)
                {
                    Settings.Type = value;
                    OnPropertyChanged("Type");
                }
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
        // deletion name binding from ui
        public string DeletionName
        {
            get { return Settings.DeletionName; }
            set
            {
                if (Settings.DeletionName != value)
                {
                    Settings.DeletionName = value;
                    OnPropertyChanged("DeletionName");
                }
            }
        }

    }
}
