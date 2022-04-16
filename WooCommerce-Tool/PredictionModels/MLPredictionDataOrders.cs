using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.PredictionModels
{
    // order ML regresion prediction data structure
    public class MLPredictionDataOrders
    {
        public string MethodName { get; set; }
        [ColumnName("Score")]
        public float OrderCount { get; set; }
    }
}
