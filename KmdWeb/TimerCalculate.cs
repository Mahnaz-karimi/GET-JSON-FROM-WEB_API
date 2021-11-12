using System;
using System.Configuration;

namespace KmdWeb
{
    public class TimerCalculate
    {
        public static double interval_event_sec = 180.0; // seconds for timer-events, database update time
        public static int reload_time_sec = 3; // time for reload data from sql database 3 secend
        public static int day_sec = 86400; // secends in day
        public static int hour_sec = 3600; //secendes in hour

        public static Double getDiff_Json_SQl(DateTime jsonUpdatedTime)
        {
            double difference_Json_SQl = Convert.ToDouble((jsonUpdatedTime - DataHandling.getLastDateTimeFromDB(ConfigurationManager.AppSettings["connectionString"])).TotalSeconds); // difference between json and sql-database
            Console.WriteLine("\n Difference between valutakurser website and our sql databases is: " + difference_Json_SQl  / 60  + " minetes \n");
            
            return difference_Json_SQl;
        }

        public static Double getDiff_Json_Now(DateTime update_At)
        {
            Double dif = Convert.ToDouble((DateTime.Now - update_At).TotalSeconds);
            Console.WriteLine("\n Valutakurser website is updated " + dif / 60  + " minetes ago, Datetime Now: " + DateTime.Now + "\n");
            return dif;
        }

        public static Double get_New_Timer_Time(Double jsont_diff_from_now) //  get new timer time for events.
        {
            if (jsont_diff_from_now <= (interval_event_sec + reload_time_sec)) // if the time difference between json and now is less or equal to the time that is estimated, we get timer on 3 min or less
            {
                return (interval_event_sec - jsont_diff_from_now) + reload_time_sec; ;
            }
            else if (jsont_diff_from_now > interval_event_sec && jsont_diff_from_now <= day_sec) // if the time difference between json and datetime-now is bigger than the time is estimated and less than one day, time will set 2 second
            {
                Console.WriteLine("\n Website currency exchange rate is not updated: " + jsont_diff_from_now / 60  + " minutter, Date time now: " + DateTime.Now);
                return reload_time_sec + interval_event_sec;
            }
            else if (day_sec < jsont_diff_from_now && jsont_diff_from_now < (day_sec * 7)) // if the time difference between json and datetime-now is bigger than one day and less to days, timer will sat one hour
            {
                Console.WriteLine("\n " + jsont_diff_from_now / day_sec + " day has passed and the website has not been updated long, about: " + jsont_diff_from_now / 60  + " minutter ago, Date time now: " + DateTime.Now);
                return hour_sec; // interval sets to one hour
            }
            else { return day_sec; }
        }

        public static Boolean If_There_Is_Difference(Double diff_Json_SQl)
        {
            if (diff_Json_SQl > 0) // if there is time difference between json and sql, data will be saved.
            {
                Console.WriteLine("\n Yes There is some thing new on the website");
                return true;
            }
            return false;
        }
                
    }
}
