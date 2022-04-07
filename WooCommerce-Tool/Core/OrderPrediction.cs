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
using WooCommerce_Tool.Core;

namespace WooCommerce_Tool
{
    public class OrderPrediction : Prediction
    {
        private Orders Orders { get; set; }
        private List<Order> OrdersData { get; set; }
        IDataView OrdersDataFull { get; set; }
        IDataView OrdersDataTrain { get; set; }
        IDataView OrdersDatatest { get; set; }
        public IEnumerable<OrdersMontlyData> SortedOrdersData { get; set; }
        private PredictionConstants Constants { get; set; }
        private MLContext mlContext { get; set; }
        public OrderPredictionSettings Settings { get; set; }
        private OrderGenerationSettingsConstants GenerationConstants { get; set; }
        public OrderPrediction(Orders orders)
        {
            Orders = orders;
            Constants = new PredictionConstants();
            mlContext = new MLContext();
        }
        // get data for predictons
        // by start, end dates and by other filter selected in ui
        public void GetData(OrderPredictionSettings settings)
        {
            GenerationConstants = new OrderGenerationSettingsConstants();
            Settings = settings;
            OrdersData = Orders.OrdersData;
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
        // get popular timeOfTheMonth
        public void OrdersMonthTimeProbability()
        {
            List<OrdersMonthTimeProbability> orders = (from p in OrdersData
                                                       group p by new { MonthPeriod = GetMonthPeriod(p.customer_note) } into d
                                                       select new OrdersMonthTimeProbability { MonthPeriod = d.Key.MonthPeriod, OrdersCount = d.Count() })
                          .OrderBy(g => g.MonthPeriod).ToList();
            // send data to ui
            Messenger.Default.Send<List<OrdersMonthTimeProbability>>(orders);

        }
        // get popular timeOfTheDay
        public void OrdersTimeProbability()
        {
            List<OrdersTimeProbability> orders = (from p in OrdersData
                                                  group p by new { Time = TimeInterval(p.customer_note) } into d
                                                  select new OrdersTimeProbability { Time = d.Key.Time, OrdersCount = d.Count() })
                          .OrderBy(g => g.Time).ToList();
            // send data to ui
            Messenger.Default.Send<List<OrdersTimeProbability>>(orders);
        }
        // Liner regresion with NN
        public void LinerRegresionWithNeuralNetwork()
        {
            float max = SortedOrdersData.Max(x => x.OrdersCount);
            float min = SortedOrdersData.Min(x => x.OrdersCount);
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
            NNOrderData nnData = new NNOrderData();
            nnData.OrderCount = new List<double>();
            for (int i = 0; i < 6; i++)
            {
                double[] forecast = nn.ComputeOutputs(predictor);
                nnData.OrderCount.Add(ReNormalizeData((float)forecast[0], min, max));
                for (int j = 0; j < predictor.Length - 1; j++)
                    predictor[j] = predictor[j + 1];
                predictor[predictor.Length - 1] = forecast[0];
            }
            // send data to ui
            Messenger.Default.Send<NNOrderData>(nnData);
        }
        // convert data to array for NN
        private double[][] DataToDoubleData(IEnumerable<OrdersMontlyData> ordersData, float min, float max)
        {
            int lenght = ordersData.Count();
            double[][] data = new double[lenght - 4][];
            for (int i = 0; i < lenght - 4; i++)
            {
                data[i] = new double[5];
                data[i][0] = DataNormalization(ordersData.ElementAt(i).OrdersCount, min, max);
                data[i][1] = DataNormalization(ordersData.ElementAt(i + 1).OrdersCount, min, max);
                data[i][2] = DataNormalization(ordersData.ElementAt(i + 2).OrdersCount, min, max);
                data[i][3] = DataNormalization(ordersData.ElementAt(i + 3).OrdersCount, min, max);
                data[i][4] = DataNormalization(ordersData.ElementAt(i + 4).OrdersCount, min, max);
            }
            return data;
        }
        // time series forecasting
        public void OrdersForecasting()
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
                outputColumnName: "ForecastedOrders",
                inputColumnName: "OrdersCount",
                windowSize: WindowSize,
                seriesLength: size,
                trainSize: size,
                horizon: 6,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundOrders",
                confidenceUpperBoundColumn: "UpperBoundOrders");
            //// train model
            SsaForecastingTransformer forecaster = forecastingPipeline.Fit(OrdersDataTrain);
            //create engine
            var forecastEngine = forecaster.CreateTimeSeriesEngine<OrdersMontlyData, OrdersMontlyForecasting>(mlContext);
            // predict
            Forecast(OrdersDatatest, 3, forecastEngine, mlContext);
        }
        // time series forecasting
        private void Forecast(IDataView testData, int horizon, TimeSeriesPredictionEngine<OrdersMontlyData, OrdersMontlyForecasting> forecaster, MLContext mlContext)
        {
            OrdersMontlyForecasting forecast = forecaster.Predict();
            // send data to ui
            Messenger.Default.Send<OrdersMontlyForecasting>(forecast);
        }
        //ML prediction
        public void FindBestModelForForecasting()
        {
            //create pipeline
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "OrdersCount")
                            .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            string BestModel = null;
            double BestModelError = 1000;
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
            var pipe = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "OrdersCount")
                .Append(mlContext.Transforms.Concatenate("Features", "Year", "Month"));
            PredictionEngine<OrdersMontlyData, MLPredictionDataOrders> PredictionFunction = null;
            switch (method)
            {
                case "FastTree":
                    var pipelineFastTree = pipe.Append(mlContext.Regression.Trainers.FastTree());
                    var modelFastTree = pipelineFastTree.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionDataOrders>(modelFastTree);
                    break;
                case "FastForest":
                    var pipelineFastForest = pipe.Append(mlContext.Regression.Trainers.FastForest());
                    var modelFastForest = pipelineFastForest.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionDataOrders>(modelFastForest);
                    break;
                case "FastTreeTweedie":
                    var pipelineFastTreeTweedie = pipe.Append(mlContext.Regression.Trainers.FastTreeTweedie());
                    var modelFastTreeTweedie = pipelineFastTreeTweedie.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionDataOrders>(modelFastTreeTweedie);
                    break;
                case "LbfgsPoissonRegression":
                    var pipelineLbfgsPoissonRegression = pipe.Append(mlContext.Regression.Trainers.LbfgsPoissonRegression());
                    var modelLbfgsPoissonRegression = pipelineLbfgsPoissonRegression.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionDataOrders>(modelLbfgsPoissonRegression);
                    break;
                case "Gam":
                    var pipelineGam = pipe.Append(mlContext.Regression.Trainers.Gam());
                    var modelGam = pipelineGam.Fit(OrdersDataFull);
                    PredictionFunction = mlContext.Model.CreatePredictionEngine<OrdersMontlyData, MLPredictionDataOrders>(modelGam);
                    break;
            }
            // predict with test data
            List<MLPredictionDataOrders> Predictions = new List<MLPredictionDataOrders>();
            var testData = new OrdersMontlyData();
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
            Messenger.Default.Send<List<MLPredictionDataOrders>>(Predictions);
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
        private IEnumerable<OrdersMontlyData> RewriteDataForecasting(List<Order> data)
        {
            //group  by month
            if (Settings.Month != "All")
                data = data.Where(x => TimeOfTheMonth(x.customer_note)).ToList();
            //group by time
            if (Settings.Time != "All")
                data = data.Where(x => TimeOfTheDay(x.customer_note)).ToList();
            // group by month
            List<OrdersMontlyData> orders = (from p in data
                                             group p by new { month = Month(p.customer_note), year = year(p.customer_note) } into d
                                             select new OrdersMontlyData { Year = float.Parse(d.Key.year), Month = float.Parse(d.Key.month), OrdersCount = d.Count() })
                          .OrderBy(g => g.Year)
                          .ThenBy(g => g.Month).ToList();
            return orders;
        }
        // return TimeOfTheMonth from product data for LINQ
        private bool TimeOfTheMonth(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            int day = int.Parse(test.ToString("dd"));
            if (Settings.Month == "Beggining of the month" && day < 10)
                return true;
            if (Settings.Month == "Midlle of the month" && day > 10 && day < 20)
                return true;
            if (Settings.Month == "End of the month" && day > 20)
                return true;
            return false;
        }
        // return TimeOfTheDay from product data for LINQ
        private bool TimeOfTheDay(string date)
        {
            int hh = TimeInterval(date);
            int index = GenerationConstants.TimeConstants.FindIndex(x => x.Contains(Settings.Time));
            int time = GenerationConstants.TimeValueConstants.ElementAt(index);
            if (hh >= (time - 1) && hh < (time + 1))
                return true;

            return false;
        }
        // return HH from date
        private int TimeInterval(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return int.Parse(test.ToString("HH"));
        }
        // return  month period string
        private string GetMonthPeriod(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            int day = int.Parse(test.ToString("dd"));
            if (day < 10)
                return "Beggining of the month";
            if (day > 10 && day < 20)
                return "Midlle of the month";
            return "End of the month";
        }
        // return object date as one string
        private string ReturnDate(OrdersMontlyData data)
        {
            return data.Year.ToString() + "/" + data.Month.ToString();

        }
    }
}
