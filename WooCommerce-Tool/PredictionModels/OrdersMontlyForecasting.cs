using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool
{
    // Order timeseries prediction data structure
    internal class OrdersMontlyForecasting
    {
        public float[] ForecastedOrders { get; set; }

        public float[] LowerBoundOrders { get; set; }

        public float[] UpperBoundOrders { get; set; }
    }
}
