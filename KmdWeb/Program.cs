
using System;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program
    {
        public static Timer newTimer = new Timer();
        public static int milisecend = 1000;
        public static string databaseName = "ValutaKurser";

        public static void setNewInterval(DateTime json_update_time)
        {
            // when the program is here, it means difference Json SQL is zero and then we get new timer time for update         
            newTimer.Interval = milisecend * TimerCalculate.get_New_Timer_Time(TimerCalculate.getDiff_Json_Now(json_update_time)); // get difference from json and now and calculate interval for new timer events 
        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DateTime update_At = json.updatedAt.Value;
            DataHandling.print_Json(json, update_At); // print json file
            Double diff_Json_SQl = TimerCalculate.getDiff_Json_SQl(update_At); // difference betwwen website and our datebase
            if (TimerCalculate.If_There_Is_Difference(diff_Json_SQl)) // if there is time difference between json and sql, json-data will be saved in sql
            {
                DataHandling.insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"], databaseName);
            }           

            // when the program is here, it means difference Json SQL is zero and then we get new timer time for update         
            setNewInterval(update_At);          
        }

        public static void Main(string[] args)
        {            
            // Timer for events           
            newTimer.Elapsed += update_sql_TimerEvent; // event for timer
            newTimer.Interval = TimerCalculate.reload_time_sec * milisecend; // starter eventes
            newTimer.Start();

            while (Console.Read() != 'q')
            {
                
            };         

        }
    }
}
