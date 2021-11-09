
using System;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program

    {
        public static Timer newTimer = new Timer();
        public static int interval_event_min = 3; // the time for timer event that check json-data and update dateabase
        public static int reload_time = 2000; // time for reload data from sql database 2000 milisecend = 2 sec

        public static int get_Minut_Difference_Json_from_sql(DateTime updatedAt) // diffrence between json and sql-database
        {
            DateTime lastDateTime = DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"]);
            return Convert.ToInt32((updatedAt - lastDateTime).TotalMinutes); 
        }

        public static int get_Minut_Difference_Json_From_Now(DateTime updatedAt) // diffrence between json and datetime now
        {         
            // calculate difrenece time between last data from database and datetime from json...
            return Convert.ToInt32((DateTime.Now - updatedAt).TotalMinutes);
        }

        public static void getNewTimer(int difference)
        {
            if (difference <= interval_event_min) // if the time difference between json and datetime-now or json and sql is less or equal to the time is estimated that the website will be updated.
            {
                int newIntervalInt = ((interval_event_min - difference) * 60 * 1000) + reload_time; // after saving data, we get new interval
                newTimer.Interval = newIntervalInt;
                Console.WriteLine("\n New timer time : " + newIntervalInt / 60 / 1000 + "  Date time now: " + DateTime.Now);
            }
            else  // if the time difference between json and datetime-now is bigge then the time is estimated, timer will closes! 
            {
                Console.WriteLine("\n Website currency exchange rate not updated, is  : " + difference + " minutter, Date time now: " + DateTime.Now);
                newTimer.Close();
            }

        }


        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DataHandling.print_Json_From_URL(json); // print json file 
            int minDifference = get_Minut_Difference_Json_from_sql(json.updatedAt.Value); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + minDifference + "\n");
            if (minDifference > 0) // if there is time difference between json and sql, therefore data is saved 
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
                int differenceJN = get_Minut_Difference_Json_From_Now(json.updatedAt.Value);
                getNewTimer(differenceJN);

            }
            else if (minDifference == 0)  // if there is no diffrenece, that means if data has been saved recently! 
            {
                int diffnow = get_Minut_Difference_Json_From_Now(json.updatedAt.Value);
                getNewTimer(diffnow);                
            }
            

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
