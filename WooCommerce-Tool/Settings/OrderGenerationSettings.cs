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
//nustatymai
// data - (darbo dienos, savaitgaliai, menesio pradzia, vidurys, vidurys, tarp pabaigos pabaiga)
// laikas - (rytas,priespieciai, pietus,popiete, vakaras, vidurnaktis, naktis)
// kiekis prekiu (mediana ar vidurkis) - List arba min max
// kaina (mediana ar vidurkis) - List arba min max

// uzsakymu kiekis