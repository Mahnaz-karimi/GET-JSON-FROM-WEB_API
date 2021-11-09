
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

        public static int get_Minut_Difference_Json_from_sql(DateTime updatedAt) // diffrence between json and sql-database
        {
            DateTime lastDateTime = DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"]); // get last datetime from database ??
            return Convert.ToInt32((updatedAt - lastDateTime).TotalMinutes);
        }

        public static int get_Minut_Difference_Json_From_Now(DateTime updatedAt) // diffrence between json and datetime now
        {         
            // calculate difrenece time between last data from database and datetime from json...
            return Convert.ToInt32((DateTime.Now - updatedAt).TotalMinutes);
        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DataHandling.print_Json_From_URL(json); // print json file 
            int minDifference = get_Minut_Difference_Json_from_sql(json.updatedAt.Value); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + minDifference + "\n");

            if ( minDifference > 0) // if the time difference between json and sql is greater than 0, it means that there should be some difference, therefore data is saved 
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);                 
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
                int newIntervalInt = ((interval_time_for_save - get_Minut_Difference_Json_From_Now(json.updatedAt.Value)) * 60 * 1000) + 1000; // after saving data, we get new interval
                newTimer.Interval = newIntervalInt; 
                Console.WriteLine("\n New timer time : " + newIntervalInt / 60 / 1000 + "      Date time now: " + DateTime.Now);
            }
            else
            {
                // else if data has been saved recently, there is no diffrenece -> minDifference == 0
                int intervalInt = (interval_time_for_save - get_Minut_Difference_Json_From_Now(json.updatedAt.Value)); // new timer time 
                newTimer.Interval = (intervalInt * 60 * 1000) + 3000; 
                Console.WriteLine("\n Data has been saved recently. Interval for event wil be : " + intervalInt + " minutes" + "\n");
            }
           
        }

        public static void Main(string[] args)
        {
            // Timer for events           
            newTimer.Elapsed += update_sql_TimerEvent; // event for timer
            newTimer.Interval = 2000; // starter eventes with new interval
            newTimer.Start();
            while (Console.Read() != 'q')
            {
            };

        }
    }
}
