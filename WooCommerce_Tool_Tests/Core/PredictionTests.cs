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
    public class PredictionTests
    {
        private int id = 1;
        private string url = "http://localhost/Testing-Site/wp-json/wc/v3/";
        private string key = "ck_2e559d28784d55fb3f15a42319b4cdfea4b77e9f";
        private string secret = "cs_6cc03fa54f45ef8f25b193991bbc75fa01d04c13";
        private Main main;
        private Prediction prediction = new Prediction();
        private int min = 1;
        private int max = 10;
        public PredictionTests()
        {
            main = new Main(id, url, key, secret);
        }
        [TestMethod()]
        public void CheckDate_DatesAreEqual1()
        {
            bool actual = prediction.CheckDate("2012/05","2012/05");
            Assert.IsTrue(actual);
        }
        [TestMethod()]
        public void CheckDate_DatesAreEqual2()
        {
            bool actual = prediction.CheckDate("2012/5", "2012/05");
            Assert.IsTrue(actual);
        }
        [TestMethod()]
        public void CheckDate_DatesAreEqual3()
        {
            bool actual = prediction.CheckDate("2012/5/14", "2012/05/25");
            Assert.IsTrue(actual);
        }
        [TestMethod()]
        public void returnMonthFromLastData_Correct1()
        {
            DateTime dateTime = new DateTime(2012, 1, 1);
            string actual = prediction.returnMonthFromLastData(1, dateTime);
            string expected = "03";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void returnMonthFromLastData_Correct2()
        {
            DateTime dateTime = new DateTime(2012, 11, 1);
            string actual = prediction.returnMonthFromLastData(2, dateTime);
            string expected = "02";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void returnMonthFromLastData_Correct3()
        {
            DateTime dateTime = new DateTime(1999, 12, 1);
            string actual = prediction.returnMonthFromLastData(0, dateTime);
            string expected = "01";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void returnYearFromLastData_Correct()
        {
            DateTime dateTime = new DateTime(1999, 12, 1);
            string actual = prediction.returnYearFromLastData(0, dateTime);
            string expected = "2000";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void Month_Correct1()
        {
            
            string actual = prediction.Month("2012/12/12");
            string expected = "12";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void Month_Correct2()
        {

            string actual = prediction.Month("2012/2/12");
            string expected = "02";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void Year_Coorect()
        {

            string actual = prediction.year("2012/12/12");
            string expected = "2012";
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        public void DataNormalizationAndDenormalization_AreEqual()
        {

            float actual = prediction.DataNormalization(55, 25, 100);
            float expected = 4;
            Assert.AreEqual(expected, actual);
            actual = prediction.ReNormalizeData(4, 25, 100);
            expected = 55;
            Assert.AreEqual(expected, actual);
        }

    }
}