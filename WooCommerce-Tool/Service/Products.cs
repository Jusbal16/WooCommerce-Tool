using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool
{
    public class Products
    {
        private WCObject wc { get; set; }
        public Products(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        public async Task<List<Product>> GetAllProducts()
        {
            List<Product> products = new List<Product>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("per_page", "100");
            int pageNumber = 1;
            parameters.Add("page", pageNumber.ToString());
            bool endWhile = false;
            while (!endWhile)
            {
                var listaTemp = await wc.Product.GetAll(parameters);
                if (listaTemp.Count > 0)
                {
                    products.AddRange(listaTemp);
                    pageNumber++;
                    parameters["page"] = pageNumber.ToString();
                }
                else
                {
                    endWhile = true;
                }
            }
            return products;
        }
    }
}
