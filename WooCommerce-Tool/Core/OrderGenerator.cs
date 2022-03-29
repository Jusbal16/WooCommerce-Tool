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
        public OrderGenerator(Products products, Customers customers, Orders orders, OrderGenerationDataLists dataList)
        {
            Products = products;
            Customers = customers;
            Orders = orders;
            DataLists = dataList;
        }
        public void GenerateOrders()
        {
            
            var taskCustomers = Customers.GetAllCustomers();
            taskCustomers.Wait();
            List<Customer> customers = taskCustomers.Result;
            var taskProducts = Products.GetAllProducts();
            taskProducts.Wait();
            List<Product> products = taskProducts.Result;
            int orderCount = 0;
            int ordersPerRequest = DataLists.Constants.OrderGenerationPerRequest; ;
            List<Order> orders = new List<Order>();
            OrderBatch batchOrders = new OrderBatch();
            ChangeUIText(orderCount.ToString() + " of " + DataLists.Settings.OrderCount.ToString() + " orders added");
            for (int i = 0; i < DataLists.Settings.OrderCount; i++)
            {
                var customer = customers.ElementAt(rnd.Next(customers.Count()));
                var product = products.ElementAt(rnd.Next(products.Count()));
                // 1 customer
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

                if ((i % ordersPerRequest) == ordersPerRequest-1)
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
        public void ChangeUIText(string text)
        {
            _theValue = text;

            //raise event
            if (this.ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs()
                {
                    NewValue = _theValue
                });
        }

    }
}
//nustatymai
// data - (darbo dienos, savaitgaliai, menesio pradzia, vidurys, pabaiga)
// laikas - (dienos pradzia, vidurys, vakaras, povakaris, naktis)
// kiekis prekiu (mediana ar vidurkis) - List arba min max
// kaina (mediana ar vidurkis) - List arba min max
// regionas - (europa, azija, amerika, afrika)
// uzsakymu kiekis

