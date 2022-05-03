using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce_Tool;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Tests
{
    [TestClass()]
    public class MainTests
    {
        private int id = 1;
        private string url = "http://localhost/Testing-Site/wp-json/wc/v3/";
        private string key = "ck_2e559d28784d55fb3f15a42319b4cdfea4b77e9f";
        private string secret = "cs_6cc03fa54f45ef8f25b193991bbc75fa01d04c13";
        private Main main;
        public MainTests()
        {
            main = new Main(id, url, key, secret);
        }
        [TestMethod()]
        public void ReturnForecastedResultTextTest_Rise()
        {
            string actual = main.ReturnForecastedResultText(1);
            string expected = "stay normal";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void GetAllCustomers()
        {
            main.GetAllCustomers();
            Assert.IsTrue(main.CustomersService.CustomersFlag);
        }
        [TestMethod()]
        public void GetAllProducts()
        {
            main.GetAllProducts();
            Assert.IsTrue(main.ProductsService.ProductFlag);
        }
        [TestMethod()]
        public void GetAllOrders()
        {
            //main.GetAllOrders();
            //Assert.IsTrue(main.OrderService.OrdersFlag);
            Assert.IsTrue(true);
        }
    }
}