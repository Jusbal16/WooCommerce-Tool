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
        private Main Main { get; set; }
        public GenerateOrdersView(Main main)
        {
            Main = main;
            _viewModel = new OrderGenerationViewModel(Main.OrderGenerator);
            DataContext = _viewModel;
            OrderGenerationSettingsConstants Constants = new();
            InitializeComponent();
            // fill date
            comboBoxDate.SelectedIndex = 0;
            comboBoxTime.SelectedIndex = 0;
            comboBoxMonthSpan.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int minOrders = Main.Constants.MinOrderCountRange;
            int maxOrders = Main.Constants.MaxOrderCountRange;
            if (Main.OrderService.OrdersFlag)
                if (CheckFill())
                {
                    if (Int32.Parse(OrderCount.Text) >= minOrders && Int32.Parse(OrderCount.Text) <= maxOrders)
                    {
                        /*Main.Settings.Date = comboBoxDate.SelectedItem.ToString();
                        Main.Settings.Time = comboBoxTime.SelectedItem.ToString();
                        Main.Settings.MonthsCount = Int32.Parse((comboBoxMonthSpan.SelectedItem as ComboboxItem).Value.ToString());
                        Main.Settings.OrderCount = Int32.Parse(OrderCount.Text);*/
                        Main.Settings.Date = _viewModel.Date;
                        Main.Settings.Time = _viewModel.Time;
                        Main.Settings.MonthsCount = _viewModel.MonthsCount;
                        Main.Settings.OrderCount = _viewModel.OrderCount;
                        Main.DataLists.GenerateDataLists();
                        bool deletion = (bool)DeleteOrders.IsChecked;
                        Task.Run(() => StartGeneration(deletion));
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
                ShowMessage("Still downloading orders, please wait", "Error");
        }
        public void StartGeneration(bool deletion)
        {
            if (deletion)
            {
                _viewModel.Status = "Deleting orders started";
                Main.DeleteAllOrders();
            }
            _viewModel.Status = "Started generating orders";
            Main.GenerateOrders();
            _viewModel.Status = "Order generation ended successfully";
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
