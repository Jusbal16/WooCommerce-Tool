﻿using GalaSoft.MvvmLight.Messaging;
using Order_Generation.PredictionTimeSeries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using WooCommerce_Tool.DB_Models;
using WooCommerce_Tool.PredictionClasses;
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool.Views
{
    /// <summary>
    /// Interaction logic for ProductPredictionView.xaml
    /// </summary>
    public partial class ProductPredictionView : UserControl
    {
        private Main Main { get; set; }
        private ProductPredictionViewModel _viewModel;
        private ProductPredictionSettings Settings { get; set; }
        public ProductPredictionView(Main main)
        {
            this.Main = main;
            _viewModel = new ProductPredictionViewModel();
            Settings = new ProductPredictionSettings();
            DataContext = _viewModel;
            InitializeComponent();
            comboBoxStartDate.SelectedIndex = 0;
            comboBoxEndDate.SelectedIndex = 0;
            comboBoxCategory.SelectedIndex = 0;
            RefreshNameList();
        }
        private void Button_Click_Prediction(object sender, RoutedEventArgs e)
        {
            CleanUp();
            if (Main.OrderService.OrdersFlag && Main.ProductsService.ProductFlag)
                if (CheckFill())
                {
                    Settings.StartDate = _viewModel.StartDate;
                    Settings.EndDate = _viewModel.EndDate;
                    Settings.Category = _viewModel.Category;    
                    Task.Run(() => Predictions(Settings));
                }
                else
                {
                    ShowMessage("Not all settings are selected", "Error");
                }
            else
                ShowMessage("Still downloading data, please wait", "Error");
        }
        public void Predictions(ProductPredictionSettings settings)
        {
            _viewModel.Status = "Downloading orders";
            Main.PredGetDataProducts(settings);
            _viewModel.Status = "Getting Categories";
            Main.PredGetProductCategories();
            _viewModel.Status = "Getting Products";
            Main.PredGetPopularProducts();
            _viewModel.Status = "Forecasting started";
            Main.PredProductForecasting();
            _viewModel.Status = "Finished";
            Main.FindBestForecastingMethodProduct();
            Main.LinerRegresionWithNeuralNetworkProduct();
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
            if (comboBoxStartDate.SelectedIndex == 0 || comboBoxEndDate.SelectedIndex == 0 || comboBoxCategory.SelectedIndex == 0)
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
        public void FillCattegeoryComboBox(List<string> cat)
        {
            ObservableCollection<string> ListOfCategories = new ObservableCollection<string>(cat);
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                foreach(var cat in ListOfCategories)
                    _viewModel.CategoryComboData.Add(cat);
            });

        }
        private void comboBoxDBNames_DropDownClosed(object sender, EventArgs e)
        {
            if (comboBoxDBNames.SelectedIndex ==0)
                return;
            Clear();
            string Name = _viewModel.Name;
            ToolProduct product = Main.ReturnProductByName(Name);
            //
            comboBoxStartDate.SelectedValue = product.StartDate;   
            comboBoxEndDate.SelectedValue = product.EndDate;
            comboBoxCategory.SelectedValue = product.Category;
            //
            IEnumerable<ProductMontlyData> data = JsonSerializer.Deserialize<IEnumerable<ProductMontlyData>>(product.TotalProducts);
            Messenger.Default.Send<IEnumerable<ProductMontlyData>>(data);
            //
            NNProductData data1 = JsonSerializer.Deserialize<NNProductData>(product.NnProducts);
            Messenger.Default.Send<NNProductData>(data1);
            //
            List<ProductPopularData> data2 = JsonSerializer.Deserialize<List<ProductPopularData>>(product.ProbabilityProducts);
            Messenger.Default.Send<List<ProductPopularData>>(data2);
            //
            List<ProductCategoriesData> data3 = JsonSerializer.Deserialize<List<ProductCategoriesData>>(product.ProbabilityCategory);
            Messenger.Default.Send<List<ProductCategoriesData>>(data3);
            //
            ProductMontlyForecasting data4 = JsonSerializer.Deserialize<ProductMontlyForecasting>(product.TimeSeriesProducts);
            Messenger.Default.Send<ProductMontlyForecasting>(data4);
            //
            List<MLPredictionDataProducts> data5 = JsonSerializer.Deserialize<List<MLPredictionDataProducts>>(product.RegresionProducts);
            Messenger.Default.Send<List<MLPredictionDataProducts>>(data5);
            //
        }
        private void Clear()
        {
            comboBoxStartDate.SelectedIndex = 0;
            comboBoxEndDate.SelectedIndex = 0;
            comboBoxCategory.SelectedIndex = 0;
            Messenger.Default.Send<IEnumerable<ProductMontlyData>>(null);
            Messenger.Default.Send<NNProductData>(null);
            Messenger.Default.Send<List<ProductPopularData>>(null);
            Messenger.Default.Send<List<ProductCategoriesData>>(null);
            Messenger.Default.Send<ProductMontlyForecasting>(null);
            Messenger.Default.Send<List<MLPredictionDataProducts>>(null);
        }
        public void RefreshNameList()
        {
            _viewModel.NamesComboData.Clear();
            _viewModel.NamesComboData.Add("Select data to delete");
            comboBoxDBNames.SelectedIndex = 0;
            ObservableCollection<string> listOfName = new ObservableCollection<string>(Main.ReturnSavedPredictionsNamesOnlyProducts());
            foreach (string t in listOfName)
                _viewModel.NamesComboData.Add(t);
        }


    }
}
