
using System;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program
    {
        public static Timer newTimer = new Timer();
        public static double interval_event = 180000.0; // milliseconds for timer-events, database update time
        public static int reload_time = 2000; // time for reload data from sql database 2000 milisecend = 2 sec
        public static int day_ms = 86400000; // milisecends in day
        public static int hour_ms = 3600000; //milisecendes in hour

        public static Double get_new_IntervalInt_when_json_is_On_time(Double jsont_difference_from_now)
        {
            Double newIntervalInt = interval_event - jsont_difference_from_now + reload_time;            
            Console.WriteLine("\n New timer time: " + newIntervalInt / 60 / 1000 + "\n");
            return newIntervalInt;
        }

        public static Double getDifference_Json_SQl( DateTime jsonUpdatedTime)
        {
            double difference_Json_SQl = Convert.ToDouble((jsonUpdatedTime - DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"])).TotalMilliseconds); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + difference_Json_SQl + " ms = " + difference_Json_SQl / 60 / 1000 + " minetes \n");
            return difference_Json_SQl;
        }

        public static Double getDifference_Json_Now(DateTime update_At)
        {    
            Double dif = Convert.ToDouble((DateTime.Now - update_At).TotalMilliseconds);
            Console.WriteLine("\n Valutakurser website is updated " + dif + " ms = " + dif / 60 / 1000 + " minetes ago \n");
            return dif;
        }

        public static void getNewTimer(Double jsont_difference_from_now) //  get new timer time for events.
        {
            if (jsont_difference_from_now <= interval_event) // if the time difference between json and now is less or equal to the time that is estimated, we get timer on 3 min or less
            {
                newTimer.Interval = get_new_IntervalInt_when_json_is_On_time(jsont_difference_from_now);               
            }
            else if (jsont_difference_from_now > interval_event && jsont_difference_from_now <= day_ms) // if the time difference between json and datetime-now is bigger than the time is estimated and less than one day, time will set 2 second
            {
                Console.WriteLine("\n Website currency exchange rate is not updated: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                newTimer.Interval = reload_time;
            }
            else if (day_ms < jsont_difference_from_now && jsont_difference_from_now < (day_ms * 7)) // if the time difference between json and datetime-now is bigger than one day and less to days, timer will sat one hour
            {
                Console.WriteLine("\n "+ jsont_difference_from_now / day_ms + " day has passed and the website has not been updated: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                newTimer.Interval = hour_ms; // interval sets to one hour
            }
 
        }

        public static void save_In_Sql_If_There_Is_Difference(Double difference_Json_SQl, dynamic json)
        {
            if (difference_Json_SQl > 0) // if there is time difference between json and sql, data will be saved.
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
            }

        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DateTime update_At = json.updatedAt.Value;
            DataHandling.print_Json_From_URL(json); // print json file
            Double difference_Json_SQl = getDifference_Json_SQl(update_At); // difference betwwen website and our datebase
            save_In_Sql_If_There_Is_Difference(difference_Json_SQl, json); // if there is time difference between json and sql, json-data will be saved in sql.          
            // when the program is here, it means difference Json SQL is zero and then we get new timer time for update         
            getNewTimer(getDifference_Json_Now(update_At)); // get difference from json and now and calculate interval for new timer events

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
