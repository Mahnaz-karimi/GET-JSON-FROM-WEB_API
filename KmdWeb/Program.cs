
using Newtonsoft.Json;

using System;
using System.Data;
using System.Net;
using System.Data.SqlClient;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program

    {
        public static Timer newTimer = new Timer();
        public static int interval_time_for_save = 3; // minut time for check json-data    

        public static int getMinuttDifference() // diffrence between json and sql-database
        {
            dynamic json = DataHandling.fetchData();
            DateTime updatedAt = json.updatedAt.Value; // get datetime from json
            DateTime lastDateTime = DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"]); // get last datetime from database ??
            TimeSpan ts = updatedAt - lastDateTime; // calculate difrenece time between last data from database and datetime from json...
            int diff = Convert.ToInt32(ts.TotalMinutes);
            return diff;
        }

        public static int getMinuttDifferenceFromJSON_Now() // diffrence between json and sql-database
        {
            dynamic json = DataHandling.fetchData();
            DateTime updatedAt = json.updatedAt.Value; // get datetime from json
            
            TimeSpan ts = DateTime.Now - updatedAt; // calculate difrenece time between last data from database and datetime from json...
            int diff = Convert.ToInt32(ts.TotalMinutes);
            return diff;
        }

        public static int getTimerTime(int MinuttDifferenceTimeSpam)
        {
            int newIntervalInt;
            if (MinuttDifferenceTimeSpam <= interval_time_for_save) // difference is less than timer, we can calculate a new timer-time that will be under 'interval_time_for_save'
            {   
                if (getMinuttDifferenceFromJSON_Now() < interval_time_for_save)
                    {
                        newIntervalInt = ((interval_time_for_save - MinuttDifferenceTimeSpam ) * 60 * 1000) + 1000; //  + 1000 ms is for because interval couldn't be zero!
                        Console.WriteLine("\n New timer time : " + newIntervalInt / 60 / 1000 + "      Date time now: " + DateTime.Now);                                                                                           //  
                        return newIntervalInt; // return a new time for timer
                    }
                
            }
                  
            return 1000;            
        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DataHandling.print_Json_From_URL(json); // print json file 
            int minDifference = getMinuttDifference(); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + minDifference + "\n");
            if ( minDifference > 0) // If diffrence between json and sql is bigger then 0, that means, there should be some diffrence, Therefore data will saved 
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);                 
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
                newTimer.Interval = getTimerTime(getMinuttDifference()); // after saving data, we get difference eventes timer with new interval                
            }
            else
            {
                newTimer.Interval = getTimerTime(getMinuttDifference()); // after saving data, we get difference eventes timer with new interval
                Console.WriteLine("\n Interval is : " + newTimer.Interval.ToString());
                Console.WriteLine("\n Min Interval is  : " + newTimer.Interval / 60 / 1000);
            }
           
        }

        public static void Main(string[] args)
        {
            // Timer for events           
            newTimer.Elapsed += update_sql_TimerEvent; // event for timer
            newTimer.Interval = 1000; // starter eventes with new interval
            newTimer.Start();
            while (Console.Read() != 'q')
            {
            };

        }
    }
}
