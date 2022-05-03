using Microsoft.VisualStudio.TestTools.UnitTesting;
using WooCommerce_Tool.DB_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.DB_Models.Tests
{
    [TestClass()]
    public class tool_dbContextTests
    {
        private tool_dbContext _dbContext;
        public tool_dbContextTests()
        {
            _dbContext = new tool_dbContext();
        }
        [TestMethod()]
        public void tool_dbContextTest_AddLogin()
        {
            var LastRow = _dbContext.Set<ToolLogin>().OrderBy(x => x.Id).LastOrDefault();
            int LoginId = (int)LastRow.Id + 1;
            ToolLogin login = new ToolLogin();
            login.Id = LoginId;
            login.ApiKey = "unittest";
            login.ApiSecret = "unittest";
            login.Url = "unittest";
            var s = _dbContext.ToolLogins.Add(login);
            _dbContext.SaveChanges();
            Assert.AreEqual(LoginId, s.Entity.Id);
            //delete

            var results = _dbContext.ToolLogins.Remove(login);
            _dbContext.SaveChanges();
            Assert.AreEqual(LoginId, results.Entity.Id);
        }
        [TestMethod()]
        public void tool_dbContextTest_AddOrder()
        {
            var LastRow = _dbContext.Set<ToolOrder>().OrderBy(x => x.Id).LastOrDefault();
            int Id = (int)LastRow.Id + 1;
            ToolOrder Order = new ToolOrder();
            Order.Id = Id;
            Order.RegresionOrder = "unittest";
            Order.TimeSeriesOrder = "unittest";
            Order.TimeOfTheMonth = "unittest";
            Order.TimeOfTheDay = "unittest";
            Order.ShopId = 1;
            Order.EndDate = "unittest";
            Order.StartDate = "unittest"; 
            var s = _dbContext.ToolOrders.Add(Order);
            _dbContext.SaveChanges();
            Assert.AreEqual(Id, s.Entity.Id);
            //delete

            var results = _dbContext.ToolOrders.Remove(Order);
            _dbContext.SaveChanges();
            Assert.AreEqual(Id, results.Entity.Id);
        }
        [TestMethod()]
        public void tool_dbContextTest_AddOProduct()
        {
            var LastRow = _dbContext.Set<ToolProduct>().OrderBy(x => x.Id).LastOrDefault();
            int Id = (int)LastRow.Id + 1;
            ToolProduct Product = new ToolProduct();
            Product.Id = Id;
            Product.ShopId = 1;
            Product.EndDate = "unittest";
            Product.StartDate = "unittest";
            Product.RegresionProducts = "unittest";
            Product.TotalProducts = "unittest";
            Product.NnProducts = "unittest";
            Product.ProbabilityProducts = "unittest";
            Product.TimeSeriesProducts = "unittest";
            Product.ProbabilityCategory= "unittest";
            var s = _dbContext.ToolProducts.Add(Product);
            _dbContext.SaveChanges();
            Assert.AreEqual(Id, s.Entity.Id);
            //delete

            var results = _dbContext.ToolProducts.Remove(Product);
            _dbContext.SaveChanges();
            Assert.AreEqual(Id, results.Entity.Id);
        }

    }
}