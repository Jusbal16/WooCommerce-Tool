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
using WooCommerce_Tool.Core;
using WooCommerce_Tool.DB_Models;

namespace WooCommerce_Tool
{
    public class Main
    {
        public Orders OrderService { get; set; }
        public Customers CustomersService { get; set; }
        public Products ProductsService { get; set; }
        public OrderGenerator OrderGenerator { get; set; }
        public OrderPrediction OrderPrediction { get; set; }
        public ProductPrediction ProductPrediction { get; set; }
        public Prediction predictionClass { get; set; }
        public StorePredictions StorePredictions { get; set; }
        public Main(int id, string url, string key, string secret)
        {
            RestAPI rest = new RestAPI(url, key, secret);
            OrderService = new Orders(rest);
            CustomersService = new Customers(rest);
            ProductsService = new Products(rest);
            //data lists
            OrderGenerator = new OrderGenerator(ProductsService, CustomersService, OrderService);
            // prediction
            predictionClass = new Prediction();
            OrderPrediction = new OrderPrediction(OrderService);
            ProductPrediction = new ProductPrediction(ProductsService, OrderService);
            // 
            StorePredictions = new StorePredictions(id);
        }
        public void GenerateDataList(OrderGenerationSettings settings)
        {
            OrderGenerator.GenerateDataList(settings);
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
        public void AddToDB(ToolOrder order, ToolProduct product)
        {
            StorePredictions.AddToDB(order, product);
        }
        public List<string> ReturnSavedPredictionsNames()
        {
            return StorePredictions.ReturnSavedPredictionsNames();
        }
        public void Delete(string name)
        {
            StorePredictions.Delete(name);
        }

        public List<string> ReturnSavedPredictionsNamesOnlyOrders()
        {
            return StorePredictions.ReturnSavedPredictionsNamesOnlyOrders();
        }
        public List<string> ReturnSavedPredictionsNamesOnlyProducts()
        {
            return StorePredictions.ReturnSavedPredictionsNamesOnlyProducts();
        }
        public ToolOrder ReturnOrderByName(string name)
        {
            return StorePredictions.ReturnOrderByName(name);
        }
        public ToolProduct ReturnProductByName(string name)
        {
            return StorePredictions.ReturnProductByName(name);
        }
    }
}
