using System;
using System.Configuration;

namespace KmdWeb
{
    public class TimerCalculate
    {
        public static double interval_event = 180000.0; // milliseconds for timer-events, database update time
        public static int reload_time = 3000; // time for reload data from sql database 2000 milisecend = 2 sec
        public static int day_ms = 86400000; // milisecends in day
        public static int hour_ms = 3600000; //milisecendes in hour

        public static Double get_new_IntervalInt_when_json_is_On_time(Double jsont_difference_from_now)
        {
            Double newIntervalInt = (interval_event - jsont_difference_from_now) + reload_time;
            Console.WriteLine("\n New timer time: " + newIntervalInt / 60 / 1000 + "\n");
            return newIntervalInt;
        }

        public static Double getDifference_Json_SQl(DateTime jsonUpdatedTime)
        {
            double difference_Json_SQl = Convert.ToDouble((jsonUpdatedTime - DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"])).TotalMilliseconds); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + difference_Json_SQl + " ms = " + difference_Json_SQl / 60 / 1000 + " minetes \n");
            return difference_Json_SQl;
        }

        public static Double getDifference_Json_Now(DateTime update_At)
        {
            Double dif = Convert.ToDouble((DateTime.Now - update_At).TotalMilliseconds);
            Console.WriteLine("\n Valutakurser website is updated " + dif / 60 / 1000 + " minetes ago \n");
            return dif;
        }

        public static Double get_New_Timer_Time(Double jsont_difference_from_now) //  get new timer time for events.
        {
            if (jsont_difference_from_now <= interval_event + reload_time) // if the time difference between json and now is less or equal to the time that is estimated, we get timer on 3 min or less
            {
                return get_new_IntervalInt_when_json_is_On_time(jsont_difference_from_now);
            }
            else if (jsont_difference_from_now > interval_event && jsont_difference_from_now <= day_ms) // if the time difference between json and datetime-now is bigger than the time is estimated and less than one day, time will set 2 second
            {
                Console.WriteLine("\n Website currency exchange rate is not updated: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                return reload_time;
            }
            else if (day_ms < jsont_difference_from_now && jsont_difference_from_now < (day_ms * 7)) // if the time difference between json and datetime-now is bigger than one day and less to days, timer will sat one hour
            {
                Console.WriteLine("\n " + jsont_difference_from_now / day_ms + " day has passed and the website has not been updated: " + jsont_difference_from_now / 60 / 1000 + " minutter, Date time now: " + DateTime.Now);
                return hour_ms; // interval sets to one hour
            }
            else { return day_ms; }

        }

        public static void save_In_Sql_If_There_Is_Difference(Double difference_Json_SQl, dynamic json)
        {
            if (difference_Json_SQl > 0) // if there is time difference between json and sql, data will be saved.
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL-database is updated. Date time now: " + DateTime.Now);
            }
        }
                
    }
}
