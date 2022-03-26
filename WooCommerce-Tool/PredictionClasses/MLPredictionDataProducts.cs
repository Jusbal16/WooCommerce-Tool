using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.PredictionClasses
{
    public class MLPredictionDataProducts
    {
        public string MethodName;
        [ColumnName("Score")]
        public float MoneySpend;
    }
}
