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
        public List<DateTime> DateList = new();
        public List<DateTime> TimeList = new();
        public List<int> ProductCount = new();
        public OrderGenerationSettings Settings { get; set; }
        public OrderGenerationSettingsConstants Constants { get; set; }
        public OrderGenerationDataLists(OrderGenerationSettings settings, OrderGenerationSettingsConstants constants)
        {
            Settings = settings;
            Constants = constants;
        }
        public void GenerateDataLists()
        {
            DateList.Clear();
            TimeList.Clear();
            GenerateDateList();
            GenerateTimeList();
        }
        private void GenerateTimeList()
        {
            string time = Settings.Time;
            int orders = Settings.OrderCount;
            int i = 0;
            int n = 0;
            int ordersSK;
            int selectedIndex = -1;
            if(orders > Constants.TimeDivisionConstants.Count)
                while (orders > 0)
                {
                    if (Constants.TimeDivisionConstants.Count > i)
                        ordersSK = Convert.ToInt32(orders * Constants.TimeDivisionConstants.ElementAt(i));
                    else
                    {
                        if (selectedIndex == n)
                            n++;
                        AddDataToTimeList(orders, n);
                        break;
                    }
                    if (selectedIndex == -1)
                    {
                        selectedIndex = Constants.TimeConstants.IndexOf(time);
                        AddDataToTimeList(ordersSK, selectedIndex);
                    }
                    else 
                    {
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
        private void AddDataToTimeList(int orderCount, int index)
        {
            string time = "";
            DateTime dateTime;
            for (int i = 0; i < orderCount; i++)
            {
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
            if (orders > Constants.DateDivisionConstants.Count)
                while (orders > 0)
                {
                    if (Constants.DateDivisionConstants.Count > i)
                        ordersSK = Convert.ToInt32(orders * Constants.DateDivisionConstants.ElementAt(i));
                    else
                    {
                        if (selectedIndex == n)
                            n++;
                        AddDataToDateList(orders, n);
                        break;
                    }
                    if (selectedIndex == -1)
                    {
                        selectedIndex = Constants.DateConstants.IndexOf(date);
                        AddDataToDateList(ordersSK, selectedIndex);
                    }
                    else
                    {
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
        private void AddDataToDateList(int orderCount, int index)
        {
            string date = "";
            var cultureInfo = new CultureInfo("de-DE");
            for (int i = 0; i < orderCount; i++)
            {
                date = GetRandomMonthWithYear() + "/" + Constants.DateValueConstants.ElementAt(index).ToString();
                var dateTime = DateTime.Parse(date, cultureInfo, DateTimeStyles.NoCurrentDateDefault);
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
        private string GetRandomMonthWithYear()
        {
            int MonthsCount = Settings.MonthsCount;
            DateTime d = DateTime.Now;
            d = d.AddMonths(rnd.Next(1, MonthsCount + 1) * -1);
            return d.ToString("yyy") + "/" + d.ToString("MM");
        }
        private bool RandomBool(int porbability)
        {
            int prob = rnd.Next(100);
            return prob <= porbability;
        }
        private void PrintList()
        {
            int n = 1;
            foreach(var l in TimeList)
            {
                Console.WriteLine(n.ToString() + " " + l.ToString());
                n++;
            }
        }
    }
}
