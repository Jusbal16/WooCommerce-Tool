using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerce_Tool
{
    public class Orders
    {
        private WCObject wc { get; set; }
        public Orders(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        public async Task DeleteAllOrders()
        {
            var task = GetAllOrders(); ;
            task.Wait();
            OrderList orders = task.Result;
            foreach (Order order in orders)
            {
                Thread.Sleep(500);
                await wc.DeleteOrder((int)order.id);
            }
        }
        public async Task<OrderList> GetAllOrders()
        {
            return await wc.GetOrders();
        }
        public async Task AddOrder(Order order)
        {
            Thread.Sleep(500);
            await wc.PostOrder(order);
        }
    }
}
