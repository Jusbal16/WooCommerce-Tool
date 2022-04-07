using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool
{
    // Customer data structure for api
    public class Customers
    {
        private WCObject wc { get; set; }
        public List<Customer> CustomersData { get; set;}
        public bool CustomersFlag { get; set; }
        public Customers(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        // get all customers from api and store them
        public async Task GetAllCustomers()
        {
            List<Customer> customers = new List<Customer>();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("per_page", "100");
            int pageNumber = 1;
            parameters.Add("page", pageNumber.ToString());
            bool endWhile = false;
            while (!endWhile)
            {
                var listaTemp = await wc.Customer.GetAll(parameters);
                if (listaTemp.Count > 0)
                {
                    customers.AddRange(listaTemp);
                    pageNumber++;
                    parameters["page"] = pageNumber.ToString();
                }
                else
                {
                    endWhile = true;
                }
            }
            CustomersData = customers;
            CustomersFlag = true;
        }
    }
}
