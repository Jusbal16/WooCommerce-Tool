using GalaSoft.MvvmLight.Messaging;
using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Order_Generation.PredictionTimeSeries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Core;
using WooCommerce_Tool.PredictionClasses;
using WooCommerce_Tool.Settings;
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
        private MLContext mlContext { get; set; }
        IDataView OrdersDataFull { get; set; }
        IDataView OrdersDataTrain { get; set; }
        IDataView OrdersDatatest { get; set; }
        IEnumerable<ProductMontlyData> SortedOrdersData { get; set; }
        private PredictionConstants Constants { get; set; }
        public ProductPrediction(Products products, Customers customers, Orders orders)
        {
            Products = products;
            Customers = customers;
            Orders = orders;
            Constants = new PredictionConstants();
            mlContext = new MLContext();
        }
        public void GetData(string startDate, string endDate)
        {
/*            var task = Orders.GetAllOrders();
            task.Wait();*/
            OrdersData = Orders.OrdersData;
            var task1 = Products.GetAllProducts();
            task1.Wait();
            ProductsData = task1.Result;
            SortedOrdersData = RewriteDataForecasting(OrdersData).SkipLast(1);
            //date
            int startIndex = 0;
            int EndIndex = 0;
            int i = 0;
            string date;
            foreach (var order in SortedOrdersData)
            {
                date = ReturnDate(order);
                if (date == startDate)
                    startIndex = i;
                if (date == endDate)
                    EndIndex = i;
                i++;
            }
            SortedOrdersData = SortedOrdersData.Skip(startIndex);
            SortedOrdersData = SortedOrdersData.Take(EndIndex - startIndex + 1);
            //
            Messenger.Default.Send<IEnumerable<ProductMontlyData>>(SortedOrdersData);
            int count = SortedOrdersData.Count();
            int trainDataSize = (count * 80) / 100;
            //
            IEnumerable<ProductMontlyData> traindata = SortedOrdersData.Take(trainDataSize);
            IEnumerable<ProductMontlyData> testdata = SortedOrdersData.Skip(trainDataSize);
            OrdersDataFull = mlContext.Data.LoadFromEnumerable(SortedOrdersData);
            OrdersDataTrain = mlContext.Data.LoadFromEnumerable(traindata);
            OrdersDatatest = mlContext.Data.LoadFromEnumerable(testdata);


        }
        public void LinerRegresionWithNeuralNetwork()
        {
            double[][] data = DataToDoubleData(SortedOrdersData);
            //
            int numInput = 4; // number predictors
            int numHidden = 12;
            int numOutput = 1; // regression
            //
            NeuralNetwork nn = new NeuralNetwork(numInput, numHidden, numOutput);

            int maxEpochs = 10000;
            double learnRate = 0.01;
            //
            double[] weights = nn.Train(data, maxEpochs, learnRate);
            // 3 month prediction
            double[] predictor = new double[4];
            predictor[0] = data[data.Length - 4][1];
            predictor[1] = data[data.Length - 4][2];
            predictor[2] = data[data.Length - 4][3];
            predictor[3] = data[data.Length - 4][4];
            NNProductData nnData = new NNProductData();
            nnData.MoneySpend = new List<double>();
            for (int i = 0; i < 6; i++)
            {
                double[] forecast = nn.ComputeOutputs(predictor);
                nnData.MoneySpend.Add(forecast[0] * 1000);
                for (int j = 0; j < predictor.Length - 1; j++)
                    predictor[j] = predictor[j + 1];
                predictor[predictor.Length - 1] = forecast[0];
            }
            Messenger.Default.Send<NNProductData>(nnData);
        }
        public double[][] DataToDoubleData(IEnumerable<ProductMontlyData> ordersData)
        {
            int lenght = ordersData.Count();
            double[][] data = new double[lenght - 4][];
            for (int i = 0; i < lenght - 4; i++)
            {
                data[i] = new double[5];
                data[i][0] = ordersData.ElementAt(i).MoneySpend / 1000.0;
                data[i][1] = ordersData.ElementAt(i + 1).MoneySpend / 1000.0;
                data[i][2] = ordersData.ElementAt(i + 2).MoneySpend / 1000.0;
                data[i][3] = ordersData.ElementAt(i + 3).MoneySpend / 1000.0;
                data[i][4] = ordersData.ElementAt(i + 4).MoneySpend / 1000.0;
            }
            return data;
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
            int size = SortedOrdersData.Count();
            int WindowSize = 0;
            if (size >= 12)
                WindowSize = 4;
            else
                return;
            var forecastingPipeline = mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "ForecastedMoney",
                inputColumnName: "MoneySpend",
                windowSize: WindowSize,
                seriesLength: size,
                trainSize: size,
                horizon: 6,
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
            Messenger.Default.Send<ProductMontlyForecasting>(forecast);
        }
        public void FindBestModelForForecasting()
        {
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "MoneySpend")
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
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "MoneySpend")
                .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            PredictionEngine<ProductMontlyData, MLPredictionDataProducts> PredictionFunction = null;
            switch (method)
            {
                case "FastTree":
                    var pipelineFastTree = pipe.Append(mlContext.Regression.Trainers.FastTree());
                    var modelFastTree = pipelineFastTree.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<ProductMontlyData, MLPredictionDataProducts>(modelFastTree);
                    break;
                case "FastForest":
                    var pipelineFastForest = pipe.Append(mlContext.Regression.Trainers.FastForest());
                    var modelFastForest = pipelineFastForest.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<ProductMontlyData, MLPredictionDataProducts>(modelFastForest);
                    break;
                case "FastTreeTweedie":
                    var pipelineFastTreeTweedie = pipe.Append(mlContext.Regression.Trainers.FastTreeTweedie());
                    var modelFastTreeTweedie = pipelineFastTreeTweedie.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<ProductMontlyData, MLPredictionDataProducts>(modelFastTreeTweedie);
                    break;
                case "LbfgsPoissonRegression":
                    var pipelineLbfgsPoissonRegression = pipe.Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());
                    var modelLbfgsPoissonRegression = pipelineLbfgsPoissonRegression.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<ProductMontlyData, MLPredictionDataProducts>(modelLbfgsPoissonRegression);
                    break;
                case "Gam":
                    var pipelineGam = pipe.Append(mlContext.Regression.Trainers.Gam());
                    var modelGam = pipelineGam.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<ProductMontlyData, MLPredictionDataProducts>(modelGam);
                    break;
            }

            List<MLPredictionDataProducts> Predictions = new List<MLPredictionDataProducts>();
            var testData = new ProductMontlyData();
            for (int i = 0; i < 6; i++)
            {
                testData.Year = float.Parse(returnYearFromLastData(i));
                testData.Month = float.Parse(returnMonthFromLastData(i));
                var prediction = PredictionFunction.Predict(testData);
                prediction.MethodName = method;
                Predictions.Add(prediction);
            }
            Messenger.Default.Send<List<MLPredictionDataProducts>>(Predictions);
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
            dateTime = dateTime.AddMonths(i + 1);
            return dateTime.ToString("yyyy");
        }
        public string returnMonthFromLastData(int i)
        {
            DateTime dateTime = GetDateTime();
            dateTime = dateTime.AddMonths(i + 1);
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
        public string ReturnDate(ProductMontlyData data)
        {
            return data.Year.ToString() + "/" + data.Month.ToString();
        }
    }
}
