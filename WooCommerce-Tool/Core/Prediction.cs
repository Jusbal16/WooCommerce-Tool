using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WooCommerce_Tool.Settings;
using WooCommerceNET.WooCommerce.v3;

namespace WooCommerce_Tool.Core
{
    public class Prediction
    {
        public PredictionConstants PConstants { get; set; }
        private int min { get; set; }
        private int max { get; set; }
        public Prediction()
        {
            PConstants = new PredictionConstants();
            min = PConstants.DataNormalizationMin;
            max = PConstants.DataNormalizationMax;
        }
        public float DataNormalization(float sk, float valmin, float valmax)
        {
            return (((sk - valmin) / (valmax - valmin)) * (max - min)) + min;
            //return (sk-min)/(max-min);
        }
        public float ReNormalizeData(float sk, float valmin, float valmax)
        {
            return ((sk * (valmax - valmin)) - (min * (valmax - valmin))) / (max - min) + valmin;
        }
        public string year(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return test.ToString("yyy");
        }
        public string Month(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return test.ToString("MM");
        }
    }
}
