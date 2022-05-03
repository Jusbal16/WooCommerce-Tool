using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce_Tool.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings.Tests
{
    [TestClass()]
    public class OrderGenerationDataListsTests
    {
        private OrderGenerationDataLists _orderGenerationDataLists;
        public OrderGenerationSettings Settings;
        public OrderGenerationConstants Constants;
        public  OrderGenerationDataListsTests()
        {
            Constants = new OrderGenerationConstants();
            Settings = new OrderGenerationSettings();
            Settings.OrderCount = 200;
            Settings.Date = "End of the month";
            Settings.MonthsCount = 24;
            Settings.Time = "Lunch 13:00";
            _orderGenerationDataLists = new OrderGenerationDataLists(Settings, Constants);
        }

        [TestMethod()]
        public void GenerateDataListsTest()
        {
            _orderGenerationDataLists.GenerateDataLists();
            Assert.IsNotNull(_orderGenerationDataLists.DateList);
        }

    }
}