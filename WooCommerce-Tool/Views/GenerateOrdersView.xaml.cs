using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
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
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.ViewsModels;

namespace WooCommerce_Tool
{
    /// <summary>
    /// Interaction logic for GenerateOrders.xaml
    /// </summary>
    public partial class GenerateOrdersView : UserControl
    {
        

        private OrderGenerationViewModel _viewModel;
        private OrderGenerationConstants Constants { get; set; }
        private OrderGenerationSettings Settings { get; set; }
        private Main Main { get; set; }
        public GenerateOrdersView(Main main)
        {
            Settings = new OrderGenerationSettings();
            Main = main;
            _viewModel = new OrderGenerationViewModel(Main.OrderGenerator);
            DataContext = _viewModel;
            Constants = new OrderGenerationConstants();
            InitializeComponent();
            // fill date
            comboBoxDate.SelectedIndex = 0;
            comboBoxTime.SelectedIndex = 0;
            comboBoxMonthSpan.SelectedIndex = 0;
        }

        private void Button_Click_Generate(object sender, RoutedEventArgs e)
        {
            int minOrders = Constants.MinOrderCountRange;
            int maxOrders = Constants.MaxOrderCountRange;
            if (Main.OrderService.OrdersFlag && Main.ProductsService.ProductFlag && Main.CustomersService.CustomersFlag)
                if (CheckFill())
                {
                    if (Int32.Parse(OrderCount.Text) >= minOrders && Int32.Parse(OrderCount.Text) <= maxOrders)
                    {
                        Settings.Date = _viewModel.Date;
                        Settings.Time = _viewModel.Time;
                        Settings.MonthsCount = _viewModel.MonthsCount;
                        Settings.OrderCount = _viewModel.OrderCount;
                        //Main.DataLists.GenerateDataLists();
                        Settings.Deletion = (bool)DeleteOrders.IsChecked;
                        bool deletion = (bool)DeleteOrders.IsChecked;
                        Task.Run(() => StartGeneration(Settings));
                    }
                    else
                    {
                        ShowMessage("Order count must be higher than " + minOrders.ToString() + " and lover than " + maxOrders.ToString(), "Error");
                    }
                }
                else
                {
                    ShowMessage("Not all settings are selected", "Error");
                }
            else
                ShowMessage("Still downloading data, please wait", "Error");
        }
        // generate orders
        public void StartGeneration(OrderGenerationSettings settings)
        {
            Main.GenerateDataList(settings);
            if (settings.Deletion)
            {
                _viewModel.Status = "Deleting orders started";
                Main.DeleteAllOrders();
            }
            _viewModel.Status = "Started generating orders";
            Main.GenerateOrders();
            _viewModel.Status = "Order generation ended successfully";
        }
        // show ui messages
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
        // check if form has only int type text
        public void OnlyInt(TextCompositionEventArgs e)
        {
            int result;
            if (!(int.TryParse(e.Text, out result)))
            {
                e.Handled = true;
            }

        }
        private void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            OnlyInt(e);
        }
        // check if all forms are filled
        public bool CheckFill()
        {
            if (String.IsNullOrEmpty(OrderCount.Text))
                return false;
            if (comboBoxDate.SelectedIndex == 0 || comboBoxMonthSpan.SelectedIndex == 0 || comboBoxTime.SelectedIndex == 0)
                return false;
            return true;
        }
    }
}
