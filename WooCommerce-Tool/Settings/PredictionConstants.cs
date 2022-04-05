using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings
{
    public class PredictionConstants
    {
        public List<string> ForecastingMethods => new List<string>() { "FastTree", "FastForest", "FastTreeTweedie", "LbfgsPoissonRegression", "Gam"};
        public int DataNormalizationMin => 0;
        public int DataNormalizationMax => 10;

    }
}
