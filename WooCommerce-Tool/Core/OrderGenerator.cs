using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.ViewsModels;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool
{
    public class ValueChangedEventArgs : EventArgs
    {
        public string NewValue { get; set; }
    }
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
    public class OrderGenerator
    {
        private string _theValue;
        public event ValueChangedEventHandler ValueChanged;
        public Random rnd = new Random();
        private Products Products { get; set; }
        private Customers Customers { get; set; }
        private Orders Orders { get; set; }
        private OrderGenerationDataLists DataLists { get; set; }
        public OrderGenerationSettings Settings { get; set; }
        public OrderGenerationConstants Constants { get; set; }
        public OrderGenerator(Products products, Customers customers, Orders orders)
        {
            Constants = new OrderGenerationConstants();
            Products = products;
            Customers = customers;
            Orders = orders;
        }
        // generate order 
        public void GenerateOrders()
        {
            List<Customer> customers = Customers.CustomersData;
            List<Product> products = Products.ProductsData;
            int orderCount = 0;
            int ordersPerRequest = DataLists.Constants.OrderGenerationPerRequest; ;
            List<Order> orders = new List<Order>();
            OrderBatch batchOrders = new OrderBatch();
            // send status tu ui
            ChangeUIText(orderCount.ToString() + " of " + DataLists.Settings.OrderCount.ToString() + " orders added");
            for (int i = 0; i < DataLists.Settings.OrderCount; i++)
            {
                var customer = customers.ElementAt(rnd.Next(customers.Count()));
                var product = products.ElementAt(rnd.Next(products.Count()));
                // create order
                var order = new Order();
                order.customer_id = (ulong?)customer.id;
                WooCommerceNET.WooCommerce.v2.OrderLineItem item = new WooCommerceNET.WooCommerce.v2.OrderLineItem();
                item.product_id = (ulong?)product.id;
                item.quantity = 1;
                item.total = product.price;
                order.line_items = new List<WooCommerceNET.WooCommerce.v2.OrderLineItem>();
                order.line_items.Add(item);
                order.customer_note = DataLists.DateList.ElementAt(i).Date + DataLists.TimeList.ElementAt(i).TimeOfDay + "-Generator";
                orders.Add(order);
                Orders.AddOrder(order);
                orderCount++;
                // maximum 100 order per batch
                if ((i % ordersPerRequest) == ordersPerRequest - 1)
                {
                    batchOrders.create = orders;
                    var taskOrders = Orders.AddOrders(batchOrders);
                    taskOrders.Wait();
                    batchOrders.create.Clear();
                    orders.Clear();
                    ChangeUIText(orderCount.ToString() + " of " + DataLists.Settings.OrderCount.ToString() + " orders added");
                }
            }
            if (orders.Count() != 0)
            {
                batchOrders.create = orders;
                var taskOrders = Orders.AddOrders(batchOrders);
                taskOrders.Wait();
                ChangeUIText(orderCount.ToString() + " of " + DataLists.Settings.OrderCount.ToString() + " orders added");
            }
        }
        // send status to ui
        public void ChangeUIText(string text)
        {
            _theValue = text;
            if (this.ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs()
                {
                    NewValue = _theValue
                });
        }
        // generate data list for orders
        public void GenerateDataList(OrderGenerationSettings settings)
        {
            Settings = settings;
            DataLists = new OrderGenerationDataLists(Settings, Constants);
            DataLists.GenerateDataLists();
        }
    }
}

