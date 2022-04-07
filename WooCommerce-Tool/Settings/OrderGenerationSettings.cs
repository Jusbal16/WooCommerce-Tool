using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings
{
    public class OrderGenerationSettings
    {
        public string Date { get; set; } 
        public int MonthsCount { get; set; }
        public string Time { get; set; }
        public int OrderCount { get; set; }
        public bool Deletion { get; set; }
    }
}