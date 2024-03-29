﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool
{
    // Product data structure for api
    public class Products
    {
        private WCObject wc { get; set; }
        public List<Product> ProductsData { get; set;}
        public bool ProductFlag { get; set; }
        public Products(RestAPI rest)
        {
            wc = new WCObject(rest);
        }
        // get all products from api and store them
        public async Task GetAllProducts()
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
            ProductsData = products;
            ProductFlag = true;
        }
    }
}
