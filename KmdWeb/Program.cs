
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
        public static int interval_time_for_save = 5; // time for at save data-json in sql

        public static dynamic fetchData() // get json data and save in json object
        {
            dynamic json;
            using (WebClient wc = new WebClient())
            {
                json = JsonConvert.DeserializeObject(wc.DownloadString("https://localhost:44351/api/values")); // other project sould run first
            }
            return json;
        }
       

        public static DateTime getLastDateTimeFromDB(dynamic json, string connString)
        {
            SqlCommand cmd = new SqlCommand();
            DateTime dtLine = DateTime.MinValue;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                SqlCommand myCommand = new SqlCommand("SELECT TOP (1) [UpdatedAt] FROM [KMD].[dbo].[ValutaKurser] order by UpdatedAt Desc; ", conn);
                SqlDataReader myReader = myCommand.ExecuteReader();
                while (myReader.Read())
                {
                    dtLine = (DateTime)myReader["UpdatedAt"];
                }
            }
            return dtLine;
        }

        public static void print_Json_From_URL(dynamic json)
        {
            DateTime updatedAt = json.updatedAt.Value;
            foreach (var item in json.valutaKurser) // loop for print json-data 
            {
                Console.WriteLine("\n DATA FROM JSON: Rate:  " + string.Format("{0:0.0000000000000000}", (item.rate.Value) +
                    "   fromCurrency: " + item.fromCurrency.Value + "   toCurrency: " + item.toCurrency.Value));
            }

            Console.WriteLine("DateTime:  " + (updatedAt.ToString("yyyy-MM-dd HH:mm:ss.fffffff")));
        }

        public static void insertJsonDataInSQl(dynamic json, string connString)
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connString)) 
                {
                    conn.Open();
                    cmd.Connection = conn;
                    foreach (var item in json.valutaKurser)
                    {
                        Console.WriteLine("INSERTING DATA IN SQL..... Rate is: " + Convert.ToString(item.rate.Value));
                        cmd.CommandText = "INSERT INTO ValutaKurser (FromCurrency, ToCurrency, Rate, UpdatedAt) ";
                        cmd.CommandText += "Values ('" + item.fromCurrency.Value + "', '" + item.toCurrency.Value + "', CAST(" + Convert.ToString(item.rate.Value).Replace(',', '.') + " AS NUMERIC(25,15))," +
                                            "convert(datetime2,'" + json.updatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'))";
                        cmd.ExecuteNonQuery();
                    }
                }
        }

        public static int newTimerTime(int MinuttDifferenceTimeSpam)
        {                                   
            int newIntervalInt;
            if (MinuttDifferenceTimeSpam <= interval_time_for_save)
            {
                newIntervalInt = (interval_time_for_save - MinuttDifferenceTimeSpam) * 60 * 1000 + 1;
                Console.WriteLine("\n New timer time : " + newIntervalInt/60/1000);
                return newIntervalInt; // If there is time diference between 30 minetes return a number that should be under 30
            }
            return 1000;            
        }

        public static int getMinuttDifferenceTimeSpam()
        {
            dynamic json = fetchData();
            DateTime updatedAt = json.updatedAt.Value; // get datetime from json
            DateTime lastDateTime = getLastDateTimeFromDB(json, ConfigurationManager.AppSettings["connectionString"]); // get last datetime in database 
            TimeSpan ts = updatedAt - lastDateTime; // calculate difrenece time between last data from database and datetime from json...
            int interval = Convert.ToInt32(ts.TotalMinutes);
            Console.WriteLine("\n Difference betweeen json and sql datetime: " + interval + "\n");
            return interval;
        }

        public static void update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = fetchData();
            int minDifference = getMinuttDifferenceTimeSpam(); // difference between json and sql-database

            if ( minDifference >= interval_time_for_save) // if diff-time is bigger then time shoud data saved so save data
            {
                insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
                Console.WriteLine("\n SQL database is updated: " + DateTime.Now);
                int intervalint = newTimerTime(getMinuttDifferenceTimeSpam());
                newTimer.Interval = intervalint; // starter eventes with new interval
                Console.WriteLine("\n New interval is: " + intervalint / 60 / 1000);
            }
           
        }

        public static void Main(string[] args)
        {
            dynamic json = fetchData(); // Get data from url like json file 
            print_Json_From_URL(json); // print json file
            
            // Timer for events
            newTimer.Elapsed += update_sql_TimerEvent;
            int intervalint = newTimerTime(getMinuttDifferenceTimeSpam());
            newTimer.Interval = intervalint; // starter eventes with new interval
            newTimer.Start();
            while (Console.Read() != 'q') {                
                Console.WriteLine("\n DateTime now: " + DateTime.Now);
            };
        }
    }
}