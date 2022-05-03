using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce_Tool.PredictionModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Core.Tests
{
    [TestClass()]
    public class PredictionModelsTests
    {

        [TestMethod()]
        public void NNOrderData_test()
        {
            NNOrderData test = new NNOrderData();
            test.OrderCount = new List<double> { 1, 4, 5 };
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void MLPredictionDataOrders_test()
        {
            MLPredictionDataOrders test = new MLPredictionDataOrders();
            test.MethodName = "test";
            test.OrderCount = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void MLPredictionDataProducts_test()
        {
            MLPredictionDataProducts test = new MLPredictionDataProducts();
            test.MethodName = "test";
            test.MoneySpend = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void NNProductData_test()
        {
            NNProductData test = new NNProductData();
            test.MoneySpend = new List<double> { 1,2};
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void OrdersMonthTimeProbability_test()
        {
            OrdersMonthTimeProbability test = new OrdersMonthTimeProbability();
            test.MonthPeriod = "test";
            test.OrdersCount = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void OrdersMontlyData_test()
        {
            OrdersMontlyData test = new OrdersMontlyData();
            test.Month = 1;
            test.Year = 1;
            test.OrdersCount = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void OrdersMontlyForecasting_test()
        {
            OrdersMontlyForecasting test = new OrdersMontlyForecasting();
            test.ForecastedOrders = new float[1];
            test.LowerBoundOrders = new float[1];
            test.UpperBoundOrders = new float[1];
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void OrdersTimeProbability_test()
        {
            OrdersTimeProbability test = new OrdersTimeProbability();
            test.Time = 1;
            test.OrdersCount = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void ProductCategoriesData_test()
        {
            ProductCategoriesData test = new ProductCategoriesData();
            test.CategoryName = "test";
            test.Count = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void ProductMontlyData_test()
        {
            ProductMontlyData test = new ProductMontlyData();
            test.Month = 1;
            test.Year = 1;
            test.MoneySpend = 1;
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void ProductMontlyForecasting_test()
        {
            ProductMontlyForecasting test = new ProductMontlyForecasting();
            test.ForecastedMoney = new float[1];
            test.LowerBoundOrders = new float[1];
            test.UpperBoundOrders = new float[1];
            Assert.IsNotNull(test);
        }
        [TestMethod()]
        public void ProductPopularData_test()
        {
            ProductPopularData test = new ProductPopularData();
            test.ProductName = "test";
            test.Count = 1;
            Assert.IsNotNull(test);
        }
    }
}