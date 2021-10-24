
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
        public static dynamic fetchData()
        {
            dynamic json;
            using (WebClient wc = new WebClient())
            {
                json = JsonConvert.DeserializeObject(wc.DownloadString(ConfigurationManager.AppSettings["url"]));
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
                    cmd.CommandText = "INSERT INTO ValutaKurser (FromCurrency, ToCurrency, Rate, UpdatedAt ) ";
                    cmd.CommandText += "Values ('" + item.fromCurrency.Value + "', '" + item.toCurrency.Value + "', CAST(" + Convert.ToString(item.rate.Value).Replace(',', '.') + " AS NUMERIC(25,15))," +
                                       "convert(datetime2,'" + json.updatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'))";
                    cmd.ExecuteNonQuery();
                }
            }
            
        }


        public static void fetchData_TimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = fetchData();

        }


        public static void Main(string[] args)
        {
            
            int intervalInt = 108000000; // timer to run programm about 30 minuttes
            string connString = ConfigurationManager.AppSettings["connectionString"];
            dynamic json = fetchData(); // Get data from url like json file

            DateTime updatedAt = json.updatedAt.Value;  // datetime from json              
            DateTime lastDateTime = getLastDateTimeFromDB(json, connString); // get last datetime in database                                                                             

            print_Json_From_URL(json);

            // Timer for events
            Timer newTimer = new Timer();
            newTimer.Elapsed += new ElapsedEventHandler(fetchData_TimerEvent);
            newTimer.Interval = intervalInt; // insert data every 30 minuter
            newTimer.Start();
            while (updatedAt.ToString() != lastDateTime.ToString()) // Here Checks at the time from json - url is not the same as Last datetime in database. If it isn't the same, program should insert json in data base 

            {                
                Console.WriteLine("\nThe Time fromm json and sql is not the same!!  Database Datetime:  " + lastDateTime.ToString() + "  " + " Json Datetime:  " + updatedAt.ToString() + "\n\nInserting data in SQL...\n");
                insertJsonDataInSQl(json, connString);

                json = fetchData();
                updatedAt = json.updatedAt.Value;
                lastDateTime = getLastDateTimeFromDB(json, connString);
                TimeSpan ts = DateTime.Now - lastDateTime;
                Console.WriteLine("Difference MINUTES:  " + ts.TotalMinutes.ToString());
                if (ts.TotalMinutes < 5)
                {
                    intervalInt = 108000000; // Set timer for 30 minuter for events igen
                }
                else if (ts.TotalMinutes > 5 && ts.TotalMinutes < 15)
                {
                    intervalInt = 54000000; // Set timer for 15 minuter for events igen
                }
                else if (ts.TotalMinutes > 15 && ts.TotalMinutes < 25)// Set timer for 1 minuter for events igen
                {
                    intervalInt = 60000; // Set timer for 1 minuter for events igen
                }
                else
                {
                    intervalInt = 1000;
                }
            }

        }
    }
}