using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool
{
    // product prediction data structure for ML regresion
    public class ProductMontlyData
    {
        public float Year { get; set; }
        public float Month { get; set; }
        public float MoneySpend { get; set; }
    }
}
