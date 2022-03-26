using GalaSoft.MvvmLight.Messaging;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Order_Generation.PredictionTimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.PredictionClasses;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool
{
    public class ProductPrediction
    {
        private Products Products { get; set; }
        private Customers Customers { get; set; }
        private Orders Orders { get; set; }
        private List<Order> OrdersData { get; set; }
        private List<Product> ProductsData { get; set; }
        public ProductPrediction(Products products, Customers customers, Orders orders)
        {
            Products = products;
            Customers = customers;
            Orders = orders;
        }
        public void GetData()
        {
            IDictionary<int, int> productId = new Dictionary<int, int>();
            var task = Orders.GetAllOrders();
            task.Wait();
            OrdersData = task.Result;
            var task1 = Products.GetAllProducts();
            task1.Wait();
            ProductsData = task1.Result;
            int key = 0;
            foreach (var order in OrdersData)
            {
                key = (int)order.line_items.ElementAt(0).product_id;
                if (productId.ContainsKey(key))
                {
                    productId[key]++;
                }
                else
                {
                    productId.Add(key, 1);
                }
            }
            var SortedPopularProducts = from entry in productId orderby entry.Value descending select entry;
            
        }
        public void GetProducts()
        {
            List<ProductPopularData> products = (from p in OrdersData
                                                 group p by new { Name = p.line_items.ElementAt(0).name } into d
                                                    select new ProductPopularData { ProductName = d.Key.Name, Count = d.Count() })
                          .OrderBy(g => g.Count).ToList();
            Messenger.Default.Send<List<ProductPopularData>>(products);
        }
        public void GetCategories()
        {
            List<ProductCategoriesData> products = (from p in ProductsData
                                                    group p by new { Category = ReturnString(p.categories)} into d
                                                       select new ProductCategoriesData { CategoryName = d.Key.Category, Count = d.Count() })
                          .OrderBy(g => g.Count).ToList();
            Messenger.Default.Send<List<ProductCategoriesData>>(products);
        }
        public void ProductsForecasting()
        {
            MLContext mlContext = new MLContext();
            IEnumerable<ProductMontlyData> ordersData = RewriteDataForecasting(OrdersData);
            Messenger.Default.Send<IEnumerable<ProductMontlyData>>(ordersData);
            int count = ordersData.Count();
            int trainDataSize = (count * 80) / 100;
            //
            IEnumerable<ProductMontlyData> traindata = ordersData.Take(trainDataSize);
            IEnumerable<ProductMontlyData> testdata = ordersData.Skip(trainDataSize);
            IDataView OrdersDataTrain = mlContext.Data.LoadFromEnumerable(traindata);
            IDataView OrdersDatatest = mlContext.Data.LoadFromEnumerable(testdata);

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedMoney",
                inputColumnName: "MoneySpend",
                windowSize: 3,
                seriesLength: 12,
                trainSize: 12,
                horizon: 3,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundOrders",
                confidenceUpperBoundColumn: "UpperBoundOrders");
            //
            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(OrdersDataTrain);
            //
            Evaluate(OrdersDatatest, forecaster, mlContext);
            //
            var forecastEngine = forecaster.CreateTimeSeriesEngine<ProductMontlyData, ProductMontlyForecasting>(mlContext);
            //
            Forecast(OrdersDatatest, 3, forecastEngine, mlContext);

        }
        public void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            IDataView predictions = model.Transform(testData);
            //
            IEnumerable<float> actual =
                (IEnumerable<float>)mlContext.Data.CreateEnumerable<ProductMontlyData>(testData, true)
                    .Select(observed => observed.MoneySpend);
            //
            IEnumerable<float> forecast =
            mlContext.Data.CreateEnumerable<ProductMontlyForecasting>(predictions, true)
                .Select(prediction => prediction.ForecastedMoney[0]);
            //
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);
            //
            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
            //
        }
        void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ProductMontlyData, ProductMontlyForecasting> forecaster, MLContext mlContext)
        {
            ProductMontlyForecasting forecast = forecaster.Predict();
            IEnumerable<string> forecastOutput =
                mlContext.Data.CreateEnumerable<ProductMontlyData>(testData, reuseRowObject: false)
                    .Take(horizon)
                    .Select((ProductMontlyData rental, int index) =>
                    {
                        string rentalDate = rental.Year.ToString() + "/" + rental.Month.ToString();
                        float actualRentals = rental.MoneySpend;
                        float lowerEstimate = Math.Max(0, forecast.LowerBoundOrders[index]);
                        float estimate = forecast.ForecastedMoney[index];
                        float upperEstimate = forecast.UpperBoundOrders[index];
                        return $"Date: {rentalDate}\n" +
                        $"Actual Rentals: {actualRentals}\n" +
                        $"Lower Estimate: {lowerEstimate}\n" +
                        $"Forecast: {estimate}\n" +
                        $"Upper Estimate: {upperEstimate}\n";
                    });
            Messenger.Default.Send<ProductMontlyForecasting>(forecast);
        }
        public IEnumerable<ProductMontlyData> RewriteDataForecasting(List<Order> data)
        {
            List<ProductMontlyData> orders = (from p in data
                                             group p by new { month = Month(p.customer_note), year = year(p.customer_note) } into d
                                             select new ProductMontlyData { Year = float.Parse(d.Key.year), Month = float.Parse(d.Key.month), MoneySpend = d.Sum(x => (float)x.line_items.ElementAt(0).price) })
                          .OrderBy(g => g.Year)
                          .ThenBy(g => g.Month).ToList();
            return orders;
        }
        public string year(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return test.ToString("yyy");
        }
        public string Month(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return test.ToString("MM");
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
