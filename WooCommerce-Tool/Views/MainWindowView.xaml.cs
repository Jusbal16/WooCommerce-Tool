﻿using System;
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
using WooCommerce_Tool.Views;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public Main Main { get; set; }
        public GenerateOrdersView generateOrdersView { get; set; }
        public OrderPredictionView orderPredictionView { get; set; }
        public ProductPredictionView productPredictionView { get; set; }
        public StorePredictionsView storePredictionView { get; set; }
        public HomeView homeView { get; set; }
        private MainWindowViewModel _viewModel { get; set; }
        public MainWindowView(int id, string url, string key, string secret)
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            Main = new Main(id, url, key, secret);
            generateOrdersView = new GenerateOrdersView(Main);
            orderPredictionView = new OrderPredictionView(Main);
            productPredictionView = new ProductPredictionView(Main);
            storePredictionView = new StorePredictionsView(Main);
            homeView = new HomeView();
            SwitchScreen(homeView);
            Task.Run(() => GetAllData());
            // reiktu viska uzkrauti

            //Dashboard obj = new Dashboard();
            //SwitchScreen(obj);
        }
        private void btnShow_Click_HomeView(object sender, RoutedEventArgs e)
        {
            SwitchScreen(homeView);
        }

        private void btnShow_Click_ProductPredictionView(object sender, RoutedEventArgs e)
        {
            productPredictionView.RefreshNameList();
            SwitchScreen(productPredictionView);
        }
        private void btnShow_Click_OrderPredictionView(object sender, RoutedEventArgs e)
        {
            //Create_Task obj = new Create_Task(-1);
            //SwitchScreen(obj);
            orderPredictionView.RefreshNameList();
            SwitchScreen(orderPredictionView);
        }
        private void btnShow_Click_StorePredictionView(object sender, RoutedEventArgs e)
        {
            SwitchScreen(storePredictionView);
        }
        private void btnShow_Click_GenerateOrdersView(object sender, RoutedEventArgs e)
        {

            SwitchScreen(generateOrdersView);

        }
        private void btnShow_Click_Logout(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            Close();
            login.Show();
        }
        public void SwitchScreen(object sender)
        {
            var screen = ((UserControl)sender);

            if (screen != null)
            {
                StackPanelMain.Children.Clear();
                StackPanelMain.Children.Add(screen);
            }
        }
        public void GetAllData()
        {
            _viewModel.Status = "Started downloading orders";
            Main.GetAllOrders();
            _viewModel.Status = "Started downloading products";
            Main.GetAllProducts();
            _viewModel.Status = "Started downloading customers";
            Main.GetAllCustomers();
            List<string> list = Main.GetCategories();
            productPredictionView.FillCattegeoryComboBox(list);
            _viewModel.Status = "Data is ready";
        }

    }
}
