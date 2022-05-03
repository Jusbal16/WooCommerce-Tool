using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce_Tool.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Core.Tests
{
    [TestClass()]
    public class StorePredictionsTests
    {
        private int id = 1;
        private string url = "http://localhost/Testing-Site/wp-json/wc/v3/";
        private string key = "ck_2e559d28784d55fb3f15a42319b4cdfea4b77e9f";
        private string secret = "cs_6cc03fa54f45ef8f25b193991bbc75fa01d04c13";
        private Main main;
        private StorePredictions storePredictions;
        public StorePredictionsTests()
        {
            main = new Main(id, url, key, secret);
            storePredictions = new StorePredictions(1);
        }
        [TestMethod()]
        public void getPrimaryKeyOrder_ReturnValue()
        {
            int actual = storePredictions.getPrimaryKeyOrder();
            Assert.IsNotNull(actual);
        }
        [TestMethod()]
        public void getPrimaryKeyProduct_ReturnValue()
        {
            int actual = storePredictions.getPrimaryKeyProduct();
            Assert.IsNotNull(actual);
        }
    }
}