using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ML.Transforms.TimeSeries;
using WooCommerceNET.WooCommerce.v3;
using GalaSoft.MvvmLight.Messaging;
using WooCommerce_Tool.Settings;
using WooCommerce_Tool.PredictionClasses;

namespace WooCommerce_Tool
{
    public class OrderPrediction
    {
        private Products Products { get; set; }
        private Customers Customers { get; set; }
        private Orders Orders { get; set; }
        private List<Order> OrdersData { get; set; }
        IDataView OrdersDataFull { get; set; }
        IDataView OrdersDataTrain { get; set; }
        IDataView OrdersDatatest { get; set; }
        IEnumerable<OrdersMontlyData> SortedOrdersData { get; set; }
        private PredictionConstants Constants {get; set;}
        private MLContext mlContext { get; set; }
        public OrderPrediction(Products products, Customers customers, Orders orders)
        {
            Products = products;
            Customers = customers;
            Orders = orders;
            Constants = new PredictionConstants();
            mlContext = new MLContext();
        }
        public void GetData()
        {
            var task = Orders.GetAllOrders();
            task.Wait();
            OrdersData = task.Result;
            SortedOrdersData = RewriteDataForecasting(OrdersData).SkipLast(1);
            Messenger.Default.Send<IEnumerable<OrdersMontlyData>>(SortedOrdersData);
            int count = SortedOrdersData.Count();
            int trainDataSize = (count * 80) / 100;
            //
            IEnumerable<OrdersMontlyData> traindata = SortedOrdersData.Take(trainDataSize);
            IEnumerable<OrdersMontlyData> testdata = SortedOrdersData.Skip(trainDataSize);
            OrdersDataFull = mlContext.Data.LoadFromEnumerable(SortedOrdersData);
            OrdersDataTrain = mlContext.Data.LoadFromEnumerable(traindata);
            OrdersDatatest = mlContext.Data.LoadFromEnumerable(testdata);
        }
        public void OrdersMonthTimeProbability()
        {
            List<OrdersMonthTimeProbability> orders = (from p in OrdersData
                                                       group p by new { MonthPeriod = GetMonthPeriod(p.customer_note) } into d
                                                       select new OrdersMonthTimeProbability { MonthPeriod = d.Key.MonthPeriod, OrdersCount = d.Count() })
                          .OrderBy(g => g.MonthPeriod).ToList();
            Messenger.Default.Send<List<OrdersMonthTimeProbability>>(orders);

        }
        public void OrdersTimeProbability()
        {
            List<OrdersTimeProbability> orders = (from p in OrdersData
                                                  group p by new { Time = TimeInterval(p.customer_note) } into d
                                                  select new OrdersTimeProbability { Time = d.Key.Time, OrdersCount = d.Count() })
                          .OrderBy(g => g.Time).ToList();
            Messenger.Default.Send<List<OrdersTimeProbability>>(orders);
        }
        public void OrdersForecasting()
        {   

            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedOrders",
                inputColumnName: "OrdersCount",
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
            //Evaluate(OrdersDatatest, forecaster, mlContext);
            //
            var forecastEngine = forecaster.CreateTimeSeriesEngine<OrdersMontlyData, OrdersMontlyForecasting>(mlContext);
            //
            Forecast(OrdersDatatest, 3, forecastEngine, mlContext);
        }
        public void Evaluate(IDataView testData, ITransformer model, MLContext mlContext)
        {
            IDataView predictions = model.Transform(testData);
            //
            IEnumerable<float> actual =
                (IEnumerable<float>)mlContext.Data.CreateEnumerable<OrdersMontlyData>(testData, true)
                    .Select(observed => observed.OrdersCount);
            //
            IEnumerable<float> forecast =
            mlContext.Data.CreateEnumerable<OrdersMontlyForecasting>(predictions, true)
                .Select(prediction => prediction.ForecastedOrders[0]);
            //
            var metrics = actual.Zip(forecast, (actualValue, forecastValue) => actualValue - forecastValue);
            //
            var MAE = metrics.Average(error => Math.Abs(error)); // Mean Absolute Error
            var RMSE = Math.Sqrt(metrics.Average(error => Math.Pow(error, 2))); // Root Mean Squared Error
            //
            /*Console.WriteLine("Evaluation Metrics");
            Console.WriteLine("---------------------");
            Console.WriteLine($"Mean Absolute Error: {MAE:F3}");
            Console.WriteLine($"Root Mean Squared Error: {RMSE:F3}\n");*/
        }
        void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<OrdersMontlyData, OrdersMontlyForecasting> forecaster, MLContext mlContext)
        {
            OrdersMontlyForecasting forecast = forecaster.Predict();
            IEnumerable<string> forecastOutput =
                mlContext.Data.CreateEnumerable<OrdersMontlyData>(testData, reuseRowObject: false)
                    .Take(horizon)
                    .Select((OrdersMontlyData rental, int index) =>
                    {
                        string rentalDate = rental.Year.ToString() + "/" + rental.Month.ToString();
                        float actualRentals = rental.OrdersCount;
                        float lowerEstimate = Math.Max(0, forecast.LowerBoundOrders[index]);
                        float estimate = forecast.ForecastedOrders[index];
                        float upperEstimate = forecast.UpperBoundOrders[index];
                        return $"Date: {rentalDate}\n" +
                        $"Actual Rentals: {actualRentals}\n" +
                        $"Lower Estimate: {lowerEstimate}\n" +
                        $"Forecast: {estimate}\n" +
                        $"Upper Estimate: {upperEstimate}\n";
                    });
            /*Console.WriteLine("Rental Forecast");
            Console.WriteLine("---------------------");
            foreach (var prediction in forecastOutput)
            {
                Console.WriteLine(prediction);
            }*/
            Messenger.Default.Send<OrdersMontlyForecasting>(forecast);
        }
        public void FindBestModelForForecasting()
        {
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "OrdersCount")
                            .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            string BestModel = null;
            double BestModelError = 1000;
            foreach (var method in Constants.ForecastingMethods)
            {
                IDataView Prediction = null;
                switch (method)
                {
                    case "FastTree":
                        var pipelineFastTree = pipe.Append(mlContext.Regression.Trainers.FastTree());
                        var modelFastTree = pipelineFastTree.Fit(OrdersDataTrain);
                        Prediction = modelFastTree.Transform(OrdersDatatest);
                        break;
                    case "FastForest":
                        var pipelineFastForest = pipe.Append(mlContext.Regression.Trainers.FastForest());
                        var modelFastForest = pipelineFastForest.Fit(OrdersDataTrain);
                        Prediction = modelFastForest.Transform(OrdersDatatest);
                        break;
                    case "FastTreeTweedie":
                        var pipelineFastTreeTweedie = pipe.Append(mlContext.Regression.Trainers.FastTreeTweedie());
                        var modelFastTreeTweedie = pipelineFastTreeTweedie.Fit(OrdersDataTrain);
                        Prediction = modelFastTreeTweedie.Transform(OrdersDatatest);
                        break;
                    case "LbfgsPoissonRegression":
                        var pipelineLbfgsPoissonRegression = pipe.Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());
                        var modelLbfgsPoissonRegression = pipelineLbfgsPoissonRegression.Fit(OrdersDataTrain);
                        Prediction = modelLbfgsPoissonRegression.Transform(OrdersDatatest);
                        break;
                    case "Gam":
                        var pipelineGam = pipe.Append(mlContext.Regression.Trainers.Gam());
                        var modelGam = pipelineGam.Fit(OrdersDataTrain);
                        Prediction = modelGam.Transform(OrdersDatatest);
                        break;
                }
                CalculateError(ref BestModel, ref BestModelError, Prediction, method);
            }
            ForecastML(BestModel);
        }
        public void ForecastML(string method)
        {
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "OrdersCount")
                .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            PredictionEngine<OrdersMontlyData, MLPredictionData> PredictionFunction = null;
            switch (method)
            {
                case "FastTree":
                    var pipelineFastTree = pipe.Append(mlContext.Regression.Trainers.FastTree());
                    var modelFastTree = pipelineFastTree.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionData>(modelFastTree);
                    break;
                case "FastForest":
                    var pipelineFastForest = pipe.Append(mlContext.Regression.Trainers.FastForest());
                    var modelFastForest = pipelineFastForest.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionData>(modelFastForest);
                    break;
                case "FastTreeTweedie":
                    var pipelineFastTreeTweedie = pipe.Append(mlContext.Regression.Trainers.FastTreeTweedie());
                    var modelFastTreeTweedie = pipelineFastTreeTweedie.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionData>(modelFastTreeTweedie);
                    break;
                case "LbfgsPoissonRegression":
                    var pipelineLbfgsPoissonRegression = pipe.Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());
                    var modelLbfgsPoissonRegression = pipelineLbfgsPoissonRegression.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionData>(modelLbfgsPoissonRegression);
                    break;
                case "Gam":
                    var pipelineGam = pipe.Append(mlContext.Regression.Trainers.Gam());
                    var modelGam = pipelineGam.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionData>(modelGam);
                    break;
            }

            List< MLPredictionData > Predictions = new List< MLPredictionData >();
            var testData = new OrdersMontlyData();
            for (int i = 0; i < 3; i++)
            {
                testData.Year = float.Parse(returnYearFromLastData(i));
                testData.Month = float.Parse(returnMonthFromLastData(i));
                var prediction = PredictionFunction.Predict(testData);
                prediction.MethodName = method;
                Predictions.Add(prediction);
            }
            Messenger.Default.Send<List<MLPredictionData>>(Predictions);
        }
        public void CalculateError(ref string model, ref double error, IDataView prediction, string modelName)
        {
            var metrics = mlContext.Regression.Evaluate(prediction, "Label", "Score");
            double ForecastingError = Math.Abs(metrics.RSquared) + Math.Abs(metrics.RootMeanSquaredError);
            if (ForecastingError < error && metrics.RSquared != 0 && metrics.RootMeanSquaredError != 0)
            {
                model = modelName;
                error = ForecastingError;
            }

        }
        public string returnYearFromLastData(int i)
        {
            DateTime dateTime = GetDateTime();
            dateTime.AddMonths(i+1);
            return dateTime.ToString("yyyy");
        }
        public string returnMonthFromLastData(int i)
        {
            DateTime dateTime = GetDateTime();
            dateTime.AddMonths(i + 1);
            return dateTime.ToString("MM");
        }
        DateTime GetDateTime()
        {
            int count = SortedOrdersData.Count();
            string year = SortedOrdersData.ElementAt(count - 3).Year.ToString();
            string Month = SortedOrdersData.ElementAt(count - 3).Month.ToString();
            DateTime dateTime = DateTime.Parse(year + "/" + Month);
            return dateTime;
        }
        public IEnumerable<OrdersMontlyData> RewriteDataForecasting(List<Order> data)
        {
            List<OrdersMontlyData> orders = (from p in data
                          group p by new { month = Month(p.customer_note), year = year(p.customer_note) } into d
                          select new OrdersMontlyData{ Year = float.Parse(d.Key.year), Month = float.Parse(d.Key.month), OrdersCount = d.Count() })
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
        public int TimeInterval(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return int.Parse(test.ToString("HH"));
        }
        public string GetMonthPeriod(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            int day = int.Parse(test.ToString("dd"));
            if (day < 10)
                return "Beggining of the month";
            if (day > 10 && day < 20)
                return "Midlle of the month";
            return "End of the month";
        }

    }
}
