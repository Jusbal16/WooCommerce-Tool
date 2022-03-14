using WooCommerce_Tool.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3.Extension;
using WooCommerceNET.WooCommerce.Legacy;
using WooCommerce.NET.WordPress.v2;

namespace WooCommerce_Tool
{
    public class Main
    {
        public Orders OrderService { get; set; }
        public Customers CustomersService { get; set; }
        public Products ProductsService { get; set; }
        public OrderGenerationSettings Settings { get; set; }
        public OrderGenerationSettingsConstants Constants { get; set; }
        public OrderGenerationDataLists DataLists { get; set; }

        public Main()
        {
            //init data
            string myShopifyUrl = "https://test-bakis.myshopify.com";
            string token = "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";

            RestAPI rest = new RestAPI("http://localhost/Testing-Site/wc-api/v3", "ck_2e559d28784d55fb3f15a42319b4cdfea4b77e9f", "cs_6cc03fa54f45ef8f25b193991bbc75fa01d04c13");
            WCObject wc = new WCObject(rest);
            OrderService = new(rest);
            CustomersService = new(rest);
            ProductsService = new(rest);
            //data lists
            Settings = new OrderGenerationSettings();
            Constants = new OrderGenerationSettingsConstants();
            DataLists = new OrderGenerationDataLists(Settings, Constants);
        }
        public void DeleteAllOrders()
        {
            var task = OrderService.DeleteAllOrders();
            task.Wait();
        }
        public void GenerateOrders()
        {
            OrderGenerator orderGenerator = new(ProductsService, CustomersService, OrderService, DataLists);
            orderGenerator.GenerateOrders();
        }
    }
}
