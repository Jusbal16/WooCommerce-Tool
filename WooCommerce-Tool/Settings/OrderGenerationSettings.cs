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
        public int ProductCount { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
        public string Region { get; set; }
        public int OrderCount { get; set; }

        public OrderGenerationSettings()
        {

        }
    }
}
//nustatymai
// data - (darbo dienos, savaitgaliai, menesio pradzia, vidurys, vidurys, tarp pabaigos pabaiga)
// laikas - (rytas,priespieciai, pietus,popiete, vakaras, vidurnaktis, naktis)
// kiekis prekiu (mediana ar vidurkis) - List arba min max
// kaina (mediana ar vidurkis) - List arba min max

// uzsakymu kiekis