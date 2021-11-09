
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

        public static void getNewTimer(int jsont_difference)
        {
            if (jsont_difference <= interval_event_min) // here the program get new timer time for events. if the time difference between json and now is less or equal to the time that is estimated, the website will be updated:
            {
                int newIntervalInt = ((interval_event_min - jsont_difference) * 60 * 1000) + reload_time; // after saving data, we get new interval
                newTimer.Interval = newIntervalInt;
                Console.WriteLine("\n New timer time : " + newIntervalInt / 60 / 1000 + "  Date time now: " + DateTime.Now);
            }
            else  // if the time difference between json and datetime-now is bigge then the time is estimated, timer will closes! 
            {
                Console.WriteLine("\n Website currency exchange rate not updated, is  : " + jsont_difference + " minutter, Date time now: " + DateTime.Now);
                newTimer.Close();
            }

        }


        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DataHandling.print_Json_From_URL(json); // print json file 
            int difference_Json_SQl = get_Minut_Difference_Json_from_sql(json.updatedAt.Value); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + difference_Json_SQl + "\n");
            int difference_Json_NOW = get_Minut_Difference_Json_From_Now(json.updatedAt.Value);

            if (difference_Json_SQl > 0) // if there is time difference between json and sql, therefore data will saved 
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);                

            }
            getNewTimer(difference_Json_NOW);


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
