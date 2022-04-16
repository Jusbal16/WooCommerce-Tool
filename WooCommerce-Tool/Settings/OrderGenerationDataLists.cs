using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce_Tool.Settings
{
    
    public class OrderGenerationDataLists
    {

        private Random rnd = new Random();
        public List<DateTime> DateList = new List<DateTime>();
        public List<DateTime> TimeList = new List<DateTime>();
        public List<int> ProductCount = new List<int>();
        public OrderGenerationSettings Settings { get; set; }
        public OrderGenerationConstants Constants { get; set; }
        public OrderGenerationDataLists(OrderGenerationSettings settings, OrderGenerationConstants constants)
        {
            Settings = settings;
            Constants = constants;
        }
        // for new genertion delete datalists
        public void GenerateDataLists()
        {
            DateList.Clear();
            TimeList.Clear();
            GenerateDateList();
            GenerateTimeList();
        }
        // generate time list, for order prediction using constans
        // depending on dalist order will be variously distributed
        private void GenerateTimeList()
        {
            string time = Settings.Time;
            int orders = Settings.OrderCount;
            int i = 0;
            int n = 0;
            int ordersSK;
            int selectedIndex = -1;
            // if generating order count is smaller than constans than dont split data
            if (orders > Constants.TimeDivisionConstants.Count)
                while (orders > 0)
                {
                    // for last element
                    if (Constants.TimeDivisionConstants.Count > i)
                    {
                        ordersSK = Convert.ToInt32(orders * Constants.TimeDivisionConstants.ElementAt(i));
                    }
                    else
                    {
                        // skip selected element, for example if selected element is "7 morning"
                        if (selectedIndex == n)
                            n++;
                        AddDataToTimeList(orders, n);
                        break;
                    }
                    // for first element
                    if (selectedIndex == -1)
                    {
                        selectedIndex = Constants.TimeConstants.IndexOf(time);
                        AddDataToTimeList(ordersSK, selectedIndex);
                    }
                    else
                    {
                        // skip selected element
                        if (selectedIndex == n)
                            n++;
                        AddDataToTimeList(ordersSK, n);
                        n++;
                    }
                    i++;
                    orders -= ordersSK;
                }
            else
            {
                selectedIndex = Constants.TimeConstants.IndexOf(time);
                AddDataToTimeList(orders, selectedIndex);
            }

        }
        // generate time for orders by given time index from constants
        private void AddDataToTimeList(int orderCount, int index)
        {
            string time = "";
            DateTime dateTime;
            for (int i = 0; i < orderCount; i++)
            {
                // random bool 50 means 50% chance/probability of getting true or false, +- 1hour
                if (RandomBool(50))
                {
                    time = AddTimeToValidFormat(Constants.TimeValueConstants.ElementAt(index).ToString(), rnd.Next(0, 60).ToString(), rnd.Next(0, 60).ToString());
                }
                else
                {
                    time = AddTimeToValidFormat((Constants.TimeValueConstants.ElementAt(index) - 1).ToString(), rnd.Next(0, 60).ToString(), rnd.Next(0, 60).ToString());
                }
                dateTime = DateTime.ParseExact(time, "HH:mm:ss", CultureInfo.InvariantCulture);
                TimeList.Add(dateTime);
            }
        }
        private string AddTimeToValidFormat(string hh, string mm, string ss)
        {
            if (hh.Length == 1)
                hh = "0" + hh;
            if (mm.Length == 1)
                mm = "0" + mm;
            if (ss.Length == 1)
                ss = "0" + ss;
            if (hh == "24")
                hh = "00";
            return hh+":"+mm+":"+ss;
        }
        private void GenerateDateList()
        {
            string date = Settings.Date;
            int orders = Settings.OrderCount;
            int i = 0;
            int n = 0;
            int ordersSK;
            int selectedIndex = -1;
            // if generating order count is smaller than constans than dont split data
            if (orders > Constants.DateDivisionConstants.Count)
                while (orders > 0)
                {
                    // for last element
                    if (Constants.DateDivisionConstants.Count > i)
                        ordersSK = Convert.ToInt32(orders * Constants.DateDivisionConstants.ElementAt(i));
                    else
                    {
                        // skip selected element, for example if selected element is "beggining of the month"
                        if (selectedIndex == n)
                            n++;
                        AddDataToDateList(orders, n);
                        break;
                    }
                    // for first element
                    if (selectedIndex == -1)
                    {
                        selectedIndex = Constants.DateConstants.IndexOf(date);
                        AddDataToDateList(ordersSK, selectedIndex);
                    }
                    else
                    {
                        // skip selected element,
                        if (selectedIndex == n)
                            n++;
                        AddDataToDateList(ordersSK, n);
                        n++;
                    }
                    i++;
                    orders -= ordersSK;
                }
            else
            {
                selectedIndex = Constants.DateConstants.IndexOf(date);
                AddDataToDateList(orders, selectedIndex);
            }
        }
        // generate date for orders by given time index from constants
        private void AddDataToDateList(int orderCount, int index)
        {
            string date = "";
            var cultureInfo = new CultureInfo("de-DE");
            for (int i = 0; i < orderCount; i++)
            {
                //get valid datetime
                date = GetRandomMonthWithYear() + "/" + Constants.DateValueConstants.ElementAt(index).ToString();
                var dateTime = DateTime.Parse(date, cultureInfo, DateTimeStyles.NoCurrentDateDefault);
                // random bool 50 means 50% chance/probability of getting true or false, +- 1hour
                if (RandomBool(50))
                {
                    dateTime = dateTime.AddDays(rnd.Next(0, Constants.MaxDateError + 1));
                }
                else
                {
                    dateTime = dateTime.AddDays(rnd.Next(0, Constants.MaxDateError + 1) * -1);
                }
                DateList.Add(dateTime);
            }
        }
        // random month with year
        private string GetRandomMonthWithYear()
        {
            int MonthsCount = Settings.MonthsCount;
            DateTime d = DateTime.Now;
            d = d.AddMonths(rnd.Next(1, MonthsCount + 1) * -1);
            return d.ToString("yyy") + "/" + d.ToString("MM");
        }
        // returns radnom bool by probability true or false. 0 <= probability <= 100
        private bool RandomBool(int porbability)
        {
            int prob = rnd.Next(100);
            return prob <= porbability;
        }
    }
}
