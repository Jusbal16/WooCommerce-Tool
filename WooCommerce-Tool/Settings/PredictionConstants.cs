using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings
{
    public class PredictionConstants
    {
        public List<string> ForecastingMethods => new() { "FastTree", "FastForest", "FastTreeTweedie", "LbfgsPoissonRegression", "Gam"};
        
    }
}
