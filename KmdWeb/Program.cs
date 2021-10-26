
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
        public static int intervalInt = 1000; // timer to run programm about 30 minuttes

        public static dynamic fetchData() // get json data and save in json object
        {
            dynamic json;
            using (WebClient wc = new WebClient())
            {
                json = JsonConvert.DeserializeObject(wc.DownloadString(ConfigurationManager.AppSettings["url"])); // 
            }
            return json;
        }
       

        public static DateTime getLastDateTimeFromDB(dynamic json, string connString)
        {
            SqlCommand cmd = new SqlCommand();
            DateTime dtLine = DateTime.MinValue;

            using ( SqlConnection conn = new SqlConnection(connString))
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
                Console.WriteLine(" DATA FROM JSON: Rate:  " + string.Format("{0:0.0000000000000000}", (item.rate.Value) + "   DateTime:  "
                    + (updatedAt.ToString("yyyy-MM-dd HH:mm:ss.fffffff")) + "   fromCurrency: " + item.fromCurrency.Value + "   toCurrency: " + item.toCurrency.Value));

            }
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

        public static int newTimerTid()
        {
            dynamic json = fetchData();
            DateTime updatedAt = json.updatedAt.Value;
            TimeSpan ts = DateTime.Now - updatedAt ; // calculate difrenece time between last data from database and datetime from json
            Console.WriteLine("intervalInttttt: " + ts.TotalMinutes.ToString());   
            if (Convert.ToInt32(ts.TotalMinutes) > 30)
            {
                intervalInt = 60000;
                return intervalInt; // 5 secend have to wait
            }
            else
            {
                intervalInt = 30 - Convert.ToInt32(ts.TotalMinutes) * 60 * 1000;
                return intervalInt; // If there is time diference between 30 minetes return a number under 30
            }
            
            

        }

        public static void Update_sql_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = fetchData();
            DateTime updatedAt = json.updatedAt.Value;

            if (json.updatedAt.Value.ToString() != getLastDateTimeFromDB(json, ConfigurationManager.AppSettings["connectionString"]).ToString()) // Here Checks at the time from json-url is not the same as Last datetime in database. if isn't the same, program should insert json in data base 
            {
                insertJsonDataInSQl(json, ConfigurationManager.AppSettings["connectionString"]);
            }
            newTimerTid();

        }


        public static void Main(string[] args)
        {
             // timer to run programm about 30 minuttes
            dynamic json = fetchData(); // Get data from url like json file
            DateTime updatedAt = json.updatedAt.Value;  // datetime from json                                                        
            DateTime lastDateTime = getLastDateTimeFromDB(json, ConfigurationManager.AppSettings["connectionString"]); // get last datetime in database                                                                             

            print_Json_From_URL(json);

            // Timer for events
            Timer newTimer = new Timer();
            newTimer.Elapsed += new ElapsedEventHandler(Update_sql_TimerEvent);

            Console.WriteLine("intervalInt: " + intervalInt / 60 / 1000);

            newTimer.Interval = intervalInt; // insert data every 30 minuter
            newTimer.Start();
            while (Console.Read() != 'q') // Here Checks at the time from json - url is not the same as Last datetime in database. If it isn't the same, program should insert json in data base 
            {               
            }

        }
    }
}