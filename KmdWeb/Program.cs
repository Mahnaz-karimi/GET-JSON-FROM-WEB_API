
using System;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program

    {
        public static Timer newTimer = new Timer();
        public static double interval_event = 180000.0; // the time of milliseconds for timer events, time for updating the database
        public static int reload_time = 2000; // time for reload data from sql database 2000 milisecend = 2 sec

        public static void getNewTimer(Double jsont_difference_from_now) //  get new timer time for events.
        {
            if (jsont_difference_from_now <= interval_event) // if the time difference between json and now is less or equal to the time that is estimated for updating the website
            {
                Double newIntervalInt = interval_event - jsont_difference_from_now + reload_time; 
                newTimer.Interval = newIntervalInt;
                Console.WriteLine("\n New timer time : " + newIntervalInt / 60 / 1000 + "  Date time now: " + DateTime.Now + "\n");
            }
            else if (jsont_difference_from_now > interval_event && jsont_difference_from_now <= 86400000)// if the time difference between json and datetime-now is bigger than the time is estimated and less than one day, time will set 2 second
            {
                Console.WriteLine("\n Website currency exchange rate not updated, is: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                newTimer.Interval = reload_time;
            }
            else if (86400000 < jsont_difference_from_now && jsont_difference_from_now < (86400000*2))// if the time difference between json and datetime-now is bigger than one day and less to days, timer will sat one hour
            {
                Console.WriteLine("\n Website currency exchange rate not updated, is: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                newTimer.Interval = 3600000; // interval sets to one hour
            }            

        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DateTime update_At = json.updatedAt.Value;
            DataHandling.print_Json_From_URL(json); // print json file 
            double difference_Json_SQl = Convert.ToDouble((update_At - DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"])).TotalMilliseconds); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + difference_Json_SQl + " ms = "+ difference_Json_SQl/60/1000+ " minetes \n");            

            if (difference_Json_SQl > 0) // if there is time difference between json and sql, data will be saved.
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
            }
            // when the program is here, it means difference Json SQL will be zero and then we get new timer time          
            getNewTimer(Convert.ToDouble((DateTime.Now - update_At).TotalMilliseconds)); // into getNewTimer method, throws time difference between json and now
        }

        public static void Main(string[] args)
        {
            // Timer for events           
            newTimer.Elapsed += update_sql_TimerEvent; // event for timer
            newTimer.Interval = reload_time; // starter eventes
            newTimer.Start();
            while (Console.Read() != 'q')
            {
            };
        }
    }
}
