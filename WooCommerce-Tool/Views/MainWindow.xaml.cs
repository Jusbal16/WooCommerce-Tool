using System;
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

namespace WooCommerce_Tool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Main Main { get; set; }
        public GenerateOrders generateOrders { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            Main = new Main();
            generateOrders = new GenerateOrders(Main);
            // reiktu viska uzkrauti

            //Dashboard obj = new Dashboard();
            //SwitchScreen(obj);
        }
        private void btnShow_Click_1(object sender, RoutedEventArgs e)
        {
            //Dashboard obj = new Dashboard();
            //SwitchScreen(obj);
        }

        private void btnShow_Click_2(object sender, RoutedEventArgs e)
        {
            //Task_Management obj = new Task_Management();
            //SwitchScreen(obj);
        }
        private void btnShow_Click_3(object sender, RoutedEventArgs e)
        {
            //Create_Task obj = new Create_Task(-1);
            //SwitchScreen(obj);
        }
        private void btnShow_Click_4(object sender, RoutedEventArgs e)
        {
            //Profiles obj = new Profiles();
            //SwitchScreen(obj);
        }
        private void btnShow_Click_5(object sender, RoutedEventArgs e)
        {

            SwitchScreen(generateOrders);

        }
        private void btnShow_Click_6(object sender, RoutedEventArgs e)
        {
            //Settings obj = new Settings();
            //SwitchScreen(obj);
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

    }
}
