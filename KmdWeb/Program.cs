
using System;
using System.Timers;
using System.Configuration;

namespace KmdWeb
{
    public class Program
    {
        public static Timer newTimer = new Timer();
        

        public static void setNewInterval(DateTime json_update_time)
        {
            // when the program is here, it means difference Json SQL is zero and then we get new timer time for update         
            newTimer.Interval = TimerCalculate.get_New_Timer_Time(TimerCalculate.getDifference_Json_Now(json_update_time)); // get difference from json and now and calculate interval for new timer events 
        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = DataHandling.fetchData(); // fetch json from the website
            DateTime update_At = json.updatedAt.Value;
            DataHandling.print_Json_From_URL(json); // print json file
            Double difference_Json_SQl = TimerCalculate.getDifference_Json_SQl(update_At); // difference betwwen website and our datebase
            TimerCalculate.save_In_Sql_If_There_Is_Difference(difference_Json_SQl, json); // if there is time difference between json and sql, json-data will be saved in sql

            // when the program is here, it means difference Json SQL is zero and then we get new timer time for update         
            setNewInterval(update_At);          
        }

        public static void Main(string[] args)
        {            
            // Timer for events           
            newTimer.Elapsed += update_sql_TimerEvent; // event for timer
            newTimer.Interval = TimerCalculate.reload_time; // starter eventes
            newTimer.Start();
            
            while (Console.Read() != 'q')
            {
                
            };         

        }
    }
}
