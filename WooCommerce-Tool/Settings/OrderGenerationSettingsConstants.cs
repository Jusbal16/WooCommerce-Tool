using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings
{
    public class OrderGenerationSettingsConstants
    {
        public  List<double> TimeDivisionConstants => new List<double>() { 0.4, 0.4, 0.4, 0.4, 0.6, 0.7};
        public  List<string> TimeConstants => new List<string>() { "Morning 7:00", "Brunch 10:00", "Lunch 13:00", "Afternoon 15:00", "Evening 18:00","Midnight 00:00", "Night 4:00" };
        public  List<int> TimeValueConstants => new List<int>() { 7, 10, 13, 15, 18, 24, 4 };
        public  int MaxTimeError => 1;

        public  List<double> DateDivisionConstants => new List<double>() { 0.6, 0.8 };
        public  List<string> DateConstants => new List<string>() { "Beggining of the month", "Midlle of the month", "End of the month" };
        public  List<int> DateValueConstants => new List<int>() { 3, 15, 28 };
        public int MonthSpan => 24; //max 100
        public int MaxDateError => 2;
        public  int MinOrderCountRange => 1;
        public int MaxOrderCountRange => 10000;
        public int OrderGenerationPerRequest => 100; //max 100
        public int ForecastingPeriod => 3;


    }
}
