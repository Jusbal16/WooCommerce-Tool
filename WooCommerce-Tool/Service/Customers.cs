using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.Legacy;

namespace WooCommerce_Tool
{
    public class Customers
    {
        private WCObject wc { get; set; }
        public Customers(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        public async Task<CustomerList> GetAllCustomers()
        {
            return await wc.GetCustomers();
        }
    }
}
