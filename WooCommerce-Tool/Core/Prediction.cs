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
        // min order count per month
        private int min { get; set; }
        // max order count per month
        private int max { get; set; }
        public Prediction()
        {
            PConstants = new PredictionConstants();
            min = PConstants.DataNormalizationMin;
            max = PConstants.DataNormalizationMax;
        }
        // normalize numeric values, to 0-10, min-max
        public float DataNormalization(float sk, float valmin, float valmax)
        {
            return (((sk - valmin) / (valmax - valmin)) * (max - min)) + min;
        }
        // renormalize numeric values
        public float ReNormalizeData(float sk, float valmin, float valmax)
        {
            return ((sk * (valmax - valmin)) - (min * (valmax - valmin))) / (max - min) + valmin;
        }
        // return only year from datetime string
        public string year(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            return test.ToString("yyy");
        }
        // return only Month from datetime string
        public string Month(string date)
        {
            DateTimeOffset test = DateTimeOffset.Parse(date.Split('-')[0]);
            string t = test.ToString("MM");
            return test.ToString("MM");
        }
        // add 1 year for predictions. best method to use datetime, if months changes from 12 to 01
        public string returnYearFromLastData(int i, DateTime dt)
        {
            DateTime dateTime = dt.AddMonths(i + 1);
            return dateTime.ToString("yyyy");
        }
        // add 1 year for predictions. best method to use datetime, if months changes from 12 to 01
        public string returnMonthFromLastData(int i, DateTime dt)
        {
            DateTime dateTime = dt.AddMonths(i + 1);
            return dateTime.ToString("MM");
        }
        // check if 2 dates are equal. '2012/02' == '2012/2' 
        public bool CheckDate(string a, string b)
        {
            var date1 = DateTime.Parse(a);
            var date2 = DateTime.Parse(b);
            if (date1.ToString("yyyy") == date2.ToString("yyyy"))
                if (date1.ToString("MM") == date2.ToString("MM"))
                    return true;
            return false;
        }
    }
}
