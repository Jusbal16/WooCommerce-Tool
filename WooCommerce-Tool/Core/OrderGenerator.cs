using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Settings;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerce_Tool
{
    public class OrderGenerator
    {
        public Random rnd = new Random();
        private Products Products { get; set; }
        private Customers Customers { get; set; }
        private Orders Orders { get; set; }
        private OrderGenerationDataLists DataLists { get; set; }
        public OrderGenerator (Products products, Customers customers, Orders orders, OrderGenerationDataLists dataList)
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
            CustomerList customers = taskCustomers.Result;
            var taskProducts = Products.GetAllProducts();
            taskProducts.Wait();
            ProductList products = taskProducts.Result;
            for (int i = 0; i < DataLists.Settings.OrderCount; i++)
            {

                
                var customer = customers.ElementAt(rnd.Next(customers.Count()));
                var product = products.ElementAt(rnd.Next(products.Count()));
                // 1 customer
                // create order
                var order = new Order();
                order.customer_id = customer.id;
                var item = new LineItem();
                item.product_id = (int?)product.id;
                item.quantity = 1;
                item.total = product.price;
                order.note = DataLists.DateList.ElementAt(i).Date + DataLists.TimeList.ElementAt(i).TimeOfDay + "-Generator";
                var taskOrders = Orders.AddOrder(order);
                taskOrders.Wait();
            }


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

