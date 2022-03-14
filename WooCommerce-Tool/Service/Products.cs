using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerce_Tool
{
    public class Products
    {
        private WCObject wc { get; set; }
        public Products(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        public async Task<ProductList> GetAllProducts()
        {
            return await wc.GetProducts();
        }
    }
}
