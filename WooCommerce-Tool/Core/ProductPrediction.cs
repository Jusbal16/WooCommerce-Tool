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
    public class ProductPrediction : Prediction
    {
        private Products Products { get; set; }
        private Orders Orders { get; set; }
        private List<Order> OrdersData { get; set; }
        private List<Product> ProductsData { get; set; }
        private MLContext mlContext { get; set; }
        IDataView OrdersDataFull { get; set; }
        IDataView OrdersDataTrain { get; set; }
        IDataView OrdersDatatest { get; set; }
        public IEnumerable<ProductMontlyData> SortedOrdersData { get; set; }
        private PredictionConstants Constants { get; set; }
        public ProductPredictionSettings Settings { get; set; }
        public ProductPrediction(Products products, Orders orders)
        {
            Products = products;
            Orders = orders;
            Constants = new PredictionConstants();
            mlContext = new MLContext();
        }
        // get data for predictons
        // by start, end dates and by other filter selected in ui
        public void GetData(ProductPredictionSettings settings)
        {
            Settings = settings;
            OrdersData = Orders.OrdersData;
            ProductsData = Products.ProductsData;
            SortedOrdersData = RewriteDataForecasting(OrdersData).SkipLast(1);
            //date
            int startIndex = 0;
            int EndIndex = 0;
            int i = 0;
            string date;
            // reduce data by date
            foreach (var order in SortedOrdersData)
            {
                date = ReturnDate(order);
                if (CheckDate(date, Settings.StartDate))
                    startIndex = i;
                if (CheckDate(date, Settings.EndDate))
                    EndIndex = i;
                i++;
            }
            SortedOrdersData = SortedOrdersData.Skip(startIndex);
            if (EndIndex != 0)
                SortedOrdersData = SortedOrdersData.Take(EndIndex - startIndex + 1);
            //
            // send data to ui
            Messenger.Default.Send<IEnumerable<ProductMontlyData>>(SortedOrdersData);
            //
            int count = SortedOrdersData.Count();
            int trainDataSize = (count * 80) / 100;
            //
            IEnumerable<ProductMontlyData> traindata = SortedOrdersData.Take(trainDataSize);
            IEnumerable<ProductMontlyData> testdata = SortedOrdersData.Skip(trainDataSize);
            OrdersDataFull = mlContext.Data.LoadFromEnumerable(SortedOrdersData);
            OrdersDataTrain = mlContext.Data.LoadFromEnumerable(traindata);
            OrdersDatatest = mlContext.Data.LoadFromEnumerable(testdata);


        }
        // Liner regresion with NN
        public void LinerRegresionWithNeuralNetwork()
        {
            float max = SortedOrdersData.Max(x => x.MoneySpend);
            float min = SortedOrdersData.Min(x => x.MoneySpend);
            double[][] data = DataToDoubleData(SortedOrdersData, min, max);
            if (data.Length < 8)
                return;
            //
            int numInput = 4; // number predictors
            int numHidden = data.Length;
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
                nnData.MoneySpend.Add(ReNormalizeData((float)forecast[0], min, max));
                for (int j = 0; j < predictor.Length - 1; j++)
                    predictor[j] = predictor[j + 1];
                predictor[predictor.Length - 1] = forecast[0];
            }
            // send data to ui
            Messenger.Default.Send<NNProductData>(nnData);
        }
        // convert data to array for NN
        private double[][] DataToDoubleData(IEnumerable<ProductMontlyData> ordersData, float min, float max)
        {
            int lenght = ordersData.Count();
            double[][] data = new double[lenght - 4][];
            for (int i = 0; i < lenght - 4; i++)
            {
                data[i] = new double[5];
                data[i][0] = DataNormalization(ordersData.ElementAt(i).MoneySpend, min, max);
                data[i][1] = DataNormalization(ordersData.ElementAt(i + 1).MoneySpend, min, max);
                data[i][2] = DataNormalization(ordersData.ElementAt(i + 2).MoneySpend, min, max);
                data[i][3] = DataNormalization(ordersData.ElementAt(i + 3).MoneySpend, min, max);
                data[i][4] = DataNormalization(ordersData.ElementAt(i + 4).MoneySpend, min, max);
            }
            return data;
        }
        // get all popular products
        public void GetProducts()
        {
            List<ProductPopularData> products = (from p in OrdersData
                                                 group p by new { Name = p.line_items.ElementAt(0).name } into d
                                                    select new ProductPopularData { ProductName = d.Key.Name, Count = d.Count() })
                          .OrderBy(g => g.Count).ToList();
            // send data to ui
            Messenger.Default.Send<List<ProductPopularData>>(products);
        }
        // get all popular Categories
        public void GetCategories()
        {
            List<ProductCategoriesData> products = (from p in ProductsData
                                                    group p by new { Category = ReturnString(p.categories)} into d
                                                       select new ProductCategoriesData { CategoryName = d.Key.Category, Count = d.Count() })
                          .OrderBy(g => g.Count).ToList();
            // send data to ui
            Messenger.Default.Send<List<ProductCategoriesData>>(products);
        }
        // time series forecasting
        public void ProductsForecasting()
        {
            int size = SortedOrdersData.Count();
            // if data is to small, time series is not possible
            int WindowSize = 0;
            if (size >= 12)
                WindowSize = 4;
            else
                return;
            //create pipeline
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
            // train model
            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(OrdersDataTrain);
            // create engine
            var forecastEngine = forecaster.CreateTimeSeriesEngine<ProductMontlyData, ProductMontlyForecasting>(mlContext);
            // predict
            Forecast(OrdersDatatest, 3, forecastEngine, mlContext);

        }
        // time series forecasting
        private void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<ProductMontlyData, ProductMontlyForecasting> forecaster, MLContext mlContext)
        {
            ProductMontlyForecasting forecast = forecaster.Predict();
            // send data to ui
            Messenger.Default.Send<ProductMontlyForecasting>(forecast);
        }
        //ML prediction
        public void FindBestModelForForecasting()
        {
            //create pipeline
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "MoneySpend")
                            .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            string BestModel = null;
            double BestModelError = 10000;
            // go trought different models for best model
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
                // calculate each model error, return best (with smallest error)
                CalculateError(ref BestModel, ref BestModelError, Prediction, method);
            }
            // predict with best model
            ForecastML(BestModel);
        }
        // forecasting with best fitting model
        private void ForecastML(string method)
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
            // predict with test data
            List<MLPredictionDataProducts> Predictions = new List<MLPredictionDataProducts>();
            var testData = new ProductMontlyData();
            for (int i = 0; i < 6; i++)
            {
                var datetime = GetDateTime();
                testData.Year = float.Parse(returnYearFromLastData(i, datetime));
                testData.Month = float.Parse(returnMonthFromLastData(i, datetime));
                var prediction = PredictionFunction.Predict(testData);
                prediction.MethodName = method;
                Predictions.Add(prediction);
            }
            // send data to ui
            Messenger.Default.Send<List<MLPredictionDataProducts>>(Predictions);
        }
        // calculate model error with know results
        private void CalculateError(ref string model, ref double error, IDataView prediction, string modelName)
        {
            var metrics = mlContext.Regression.Evaluate(prediction, "Label", "Score");
            double ForecastingError = Math.Abs(metrics.RSquared) + Math.Abs(metrics.RootMeanSquaredError);
            if (ForecastingError < error && metrics.RSquared != 0 && metrics.RootMeanSquaredError != 0)
            {
                model = modelName;
                error = ForecastingError;
            }

        }

        // return forecasted date time
        private DateTime GetDateTime()
        {
            int count = SortedOrdersData.Count();
            string year = SortedOrdersData.ElementAt(count - 3).Year.ToString();
            string Month = SortedOrdersData.ElementAt(count - 3).Month.ToString();
            DateTime dateTime = DateTime.Parse(year + "/" + Month);
            return dateTime;
        }
        // rewrite data from api returned object, to predictions prepared object
        private IEnumerable<ProductMontlyData> RewriteDataForecasting(List<Order> data)
        {
            // group by category
            if (Settings.Category != "All")
                data = data.Where(x => ReturnCategory((long)x.line_items.ElementAt(0).product_id)).ToList();
            //
            List<ProductMontlyData> orders = (from p in data
                                              group p by new { month = Month(p.customer_note), year = year(p.customer_note) } into d
                                              select new ProductMontlyData { Year = float.Parse(d.Key.year), Month = float.Parse(d.Key.month), MoneySpend = d.Where(x => CheckIfNotNull((float)x.line_items.ElementAt(0).price) == true).Sum(x => (float)x.line_items.ElementAt(0).price) })
                          .OrderBy(g => g.Year)
                          .ThenBy(g => g.Month).ToList();
            return orders;
        }
        // check if variable has value
        private bool CheckIfNotNull(float a)
        {
            if (a != null)
                return true;
            return false;
        }
        // return category from product data for LINQ
        private bool ReturnCategory(long id)
        {
            var product = ProductsData.Single(x => (long)x.id == id);
            if (Settings.Category == ReturnString(product.categories))
                return true;
            return false;
        }
        // return combined string from list of strings
        private string ReturnString(List<ProductCategoryLine> categories)
        {
            string cat = "";
            foreach (var c in categories)
                cat += c.name.ToString() + "|";
            return cat;
        }
        // return object date as one string
        private string ReturnDate(ProductMontlyData data)
        {
            return data.Year.ToString() + "/" + data.Month.ToString();
        }
    }
}
