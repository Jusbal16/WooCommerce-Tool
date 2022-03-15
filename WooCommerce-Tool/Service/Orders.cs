﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

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
            List<Order> orders = task.Result;
            foreach (Order order in orders)
            {
                await wc.Order.Delete((int)order.id);
            }
        }
        public async Task<List<Order>> GetAllOrders()
        {
            List<Order> orders = new List<Order>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("per_page", "100");
            int pageNumber = 1;
            parameters.Add("page", pageNumber.ToString());
            bool endWhile = false;
            while (!endWhile)
            {
                var listaTemp = await wc.Order.GetAll(parameters);
                if (listaTemp.Count > 0)
                {
                    orders.AddRange(listaTemp);
                    pageNumber++;
                    parameters["page"] = pageNumber.ToString();
                }
                else
                {
                    endWhile = true;
                }
            }
            return orders;
        }
        public async Task AddOrder(Order order)
        {
            await wc.Order.Add(order);
        }
    }
}
