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

namespace WooCommerce_Tool
{
    /// <summary>
    /// Interaction logic for GenerateOrders.xaml
    /// </summary>
    public partial class GenerateOrders : UserControl
    {
        
        public class ComboboxItem
        {
            public string Text { get; set; }
            public object Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }
        public static TextBlock StatusText;
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private Main Main { get; set; }
        public GenerateOrders(Main main)
        {
            Main = main;
            OrderGenerationSettingsConstants Constants = new();
            InitializeComponent();
            StatusText = GenerationStatus;
            // fill date
            comboBoxDate.Items.Add("Select Date");
            foreach (var n in Constants.DateConstants)
                comboBoxDate.Items.Add(n);
            comboBoxDate.SelectedIndex = 0;

            comboBoxTime.Items.Add("Select Time");
            foreach (var n in Constants.TimeConstants)
                comboBoxTime.Items.Add(n);
            comboBoxTime.SelectedIndex = 0;
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "Select Month Span", Value = 0 });
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "1 Month", Value = 1 });
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "3 Month", Value = 3 });
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "6 Month", Value = 6 });
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "9 Month", Value = 9 });
            comboBoxMonthSpan.Items.Add(new ComboboxItem { Text = "12 Month", Value = 12 });
            comboBoxMonthSpan.SelectedIndex = 0;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int minOrders = Main.Constants.MinOrderCountRange;
            int maxOrders = Main.Constants.MaxOrderCountRange;
            if (CheckFill())
            {
                if (Int32.Parse(OrderCount.Text) >= minOrders && Int32.Parse(OrderCount.Text) <= maxOrders)
                {
                    Main.Settings.Date = comboBoxDate.SelectedItem.ToString();
                    Main.Settings.Time = comboBoxTime.SelectedItem.ToString();
                    Main.Settings.MonthsCount = Int32.Parse((comboBoxMonthSpan.SelectedItem as ComboboxItem).Value.ToString());
                    Main.Settings.OrderCount = Int32.Parse(OrderCount.Text);
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
        }
        public void StartGeneration(bool deletion)
        {
            var chan = ChangeUIText;
            if (deletion)
            {
                ChangeUIText("Deleting orders started");
                Main.DeleteAllOrders();
            }
            ChangeUIText("Started generating orders");
            Main.GenerateOrders(ChangeUIText);
            ChangeUIText("Order generation ended successfully");
        }
        public void ChangeUIText(string text)
        {
            Dispatcher.Invoke(() =>
            {
                // Code causing the exception or requires UI thread access
                StatusText.Text = text;
            });
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
