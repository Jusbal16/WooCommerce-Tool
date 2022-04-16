using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order_Generation.PredictionModels
{
    // product timeseries prediction data structure
    internal class ProductMontlyForecasting
    {
        public float[] ForecastedMoney { get; set; }

        public float[] LowerBoundOrders { get; set; }

        public float[] UpperBoundOrders { get; set; }
    }
}
