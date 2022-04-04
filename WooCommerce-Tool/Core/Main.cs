using WooCommerce_Tool.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerce.NET.WordPress.v2;
using WooCommerce_Tool.ViewsModels;

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
        public OrderGenerator OrderGenerator { get; set; }
        public OrderPrediction OrderPrediction { get; set; }
        public ProductPrediction ProductPrediction { get; set; }
        public OrderPredictionSettings OrderPredSettings { get; set; }
        private List<Order> OrdersData { get; set; }
        public Main()
        {
            //init data
            string myShopifyUrl = "https://test-bakis.myshopify.com";
            string token = "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";

            RestAPI rest = new RestAPI("http://localhost/Testing-Site/wp-json/wc/v3/", "ck_2e559d28784d55fb3f15a42319b4cdfea4b77e9f", "cs_6cc03fa54f45ef8f25b193991bbc75fa01d04c13");
            OrderService = new(rest);
            CustomersService = new(rest);
            ProductsService = new(rest);
            //data lists
            Settings = new OrderGenerationSettings();
            Constants = new OrderGenerationSettingsConstants();
            DataLists = new OrderGenerationDataLists(Settings, Constants);
            OrderGenerator = new(ProductsService, CustomersService, OrderService, DataLists);
            // prediction
            OrderPredSettings = new OrderPredictionSettings();
            OrderPrediction = new(ProductsService, CustomersService, OrderService);
            ProductPrediction = new(ProductsService, CustomersService, OrderService);
        }
        public void DeleteAllOrders()
        {
            var task = OrderService.DeleteAllOrders();
            task.Wait();
        }
        public void GenerateOrders()
        {
            OrderGenerator.GenerateOrders();
        }
        public void PredGetData(OrderPredictionSettings settings)
        {
            OrderPrediction.GetData(settings);
        }
        public void PredOrderForecasting()
        {
            OrderPrediction.OrdersForecasting();
        }
        public void ProbOrdersMonthTime()
        {
            OrderPrediction.OrdersMonthTimeProbability();
        }
        public void ProbOrdersTime()
        {
            OrderPrediction.OrdersTimeProbability();
        }
        public void PredGetDataProducts(ProductPredictionSettings settings)
        {
            ProductPrediction.GetData(settings);
        }
        public void PredProductForecasting()
        {
            ProductPrediction.ProductsForecasting();
        }
        public void PredGetProductCategories()
        {
            ProductPrediction.GetCategories();
        }
        public void PredGetPopularProducts()
        {
            ProductPrediction.GetProducts();
        }
        public void FindBestForecastingMethod()
        {
            OrderPrediction.FindBestModelForForecasting();
        }
        public void FindBestForecastingMethodProduct()
        {
            ProductPrediction.FindBestModelForForecasting();
        }
        public void LinerRegresionWithNeuralNetwork()
        {
            OrderPrediction.LinerRegresionWithNeuralNetwork();
        }
        public void LinerRegresionWithNeuralNetworkProduct()
        {
            ProductPrediction.LinerRegresionWithNeuralNetwork();
        }
        public void GetAllOrders()
        {
            OrderService.OrdersFlag = false;
            var task = OrderService.GetAllOrders();
            task.Wait();
        }
        public void GetAllProducts()
        {
            ProductsService.ProductFlag = false;
            var task = ProductsService.GetAllProducts();
            task.Wait();
        }
        public void GetAllCustomers()
        {
            CustomersService.CustomersFlag = false;
            var task = CustomersService.GetAllCustomers();
            task.Wait();
        }
        public List<string> GetCategories()
        {
            List<string> list = (from d in ProductsService.ProductsData
                       group d by new { Category = ReturnString(d.categories) } into p
                       orderby p.Key.Category
                       select p.Key.Category).ToList();

            return list;
        }
        public string ReturnString(List<ProductCategoryLine> categories)
        {
            string cat = "";
            foreach (var c in categories)
                cat += c.name.ToString() + "|";
            return cat;
        }

    }
}
