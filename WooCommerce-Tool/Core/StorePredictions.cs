using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.DB_Models;

namespace WooCommerce_Tool.Core
{
    public class StorePredictions
    {
        private tool_dbContext _dbContext;
        private int ShopID { get; set; }
        public StorePredictions(int shopID)
        {
            ShopID = shopID;
            _dbContext = new tool_dbContext();
        }
        // add order or product to db
        public void AddToDB(ToolOrder order, ToolProduct product)
        {
            if (order != null)
            {
                order.Id = getPrimaryKeyOrder();
                order.ShopId = ShopID;
                _dbContext.ToolOrders.Add(order);
                _dbContext.SaveChanges();
            }
            if (product != null)
            {
                product.Id = getPrimaryKeyProduct();
                product.ShopId = ShopID;
                _dbContext.ToolProducts.Add(product);
                _dbContext.SaveChanges();
            }
        }
        // return id of order
        public int getPrimaryKeyOrder()
        {
            var LastRow = _dbContext.Set<ToolOrder>().OrderBy(x => x.Id).LastOrDefault();
            if (LastRow == null)
                return 1;
            return (int)LastRow.Id + 1;
        }
        // return id of product
        public int getPrimaryKeyProduct()
        {
            var LastRow = _dbContext.Set<ToolProduct>().OrderBy(x => x.Id).LastOrDefault();
            if (LastRow == null)
                return 1;
            return (int)LastRow.Id + 1;
        }
        // return all saved predictions names
        public List<string> ReturnSavedPredictionsNames()
        {
            List<string> List = new List<string>();
            List = _dbContext.Set<ToolProduct>().Select(x => x.Name).ToList();
            List.AddRange(_dbContext.Set<ToolOrder>().Select(x => x.Name).ToList());
            return List.Distinct().ToList();
        }
        // return order saved predictions names
        public List<string> ReturnSavedPredictionsNamesOnlyOrders()
        {
            return _dbContext.Set<ToolOrder>().Select(x => x.Name).ToList();
        }
        // return product saved predictions names
        public List<string> ReturnSavedPredictionsNamesOnlyProducts()
        {
            return _dbContext.Set<ToolProduct>().Select(x => x.Name).ToList();
        }
        // delete saved prediction by name
        public void Delete(string name)
        {
            var order = _dbContext.ToolOrders.Where(x => x.Name == name).FirstOrDefault<ToolOrder>();
            var product = _dbContext.ToolProducts.Where(x => x.Name == name).FirstOrDefault<ToolProduct>();
            if (order != null)
            {
                _dbContext.ToolOrders.Remove(order);
                _dbContext.SaveChanges();
            }
            if (product != null)
            {
                _dbContext.ToolProducts.Remove(product);
                _dbContext.SaveChanges();
            }
        }
        // return order objecct by name from db
        public ToolOrder ReturnOrderByName(string name)
        {
            return _dbContext.ToolOrders.Where(x => x.Name == name).FirstOrDefault<ToolOrder>();
        }
        // return product objecct by name from db
        public ToolProduct ReturnProductByName(string name)
        {
            return _dbContext.ToolProducts.Where(x => x.Name == name).FirstOrDefault<ToolProduct>();
        }
    }
}
