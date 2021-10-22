
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

        public static void printRateAndTime(DateTime dt, decimal rate, string fromCurrency, string toCurrency)
        {
            System.Console.WriteLine(" DATA FROM JSON \n Rate:  " + string.Format("{0:0.00000000000000}", rate) + " DateTime:  " + (DateTime)(dt));
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

        public static void insertJsonDataInSQlEvent(dynamic json, string connString)
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                cmd.Connection = conn;
                foreach (var item in json.valutaKurser)
                {
                    Console.WriteLine("RATE: " + Convert.ToString(item.rate.Value));
                    cmd.CommandText = "INSERT INTO ValutaKurser (FromCurrency, ToCurrency, Rate, UpdatedAt ) ";
                    cmd.CommandText += "Values ('" + item.fromCurrency.Value + "', '" + item.toCurrency.Value + "', CAST(" + Convert.ToString(item.rate.Value).Replace(',', '.') + " AS NUMERIC(25,15)), convert(datetime2,'" + json.updatedAt.Value.ToString() + "',103))";
                    cmd.ExecuteNonQuery();

                }
            }

        }


        public static void fetchDataOnTimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = fetchData();

            if (json.updatedAt.Value.ToString() != getLastDateTimeFromDB(json, ConfigurationManager.AppSettings["connectionString"]).ToString()) // Here Checks at the time from json-url is not the same as Last datetime in database. if isn't the same, program should insert json in data base 
            {
                insertJsonDataInSQlEvent(json, ConfigurationManager.AppSettings["connectionString"]);
            }
        }


        public static void Main(string[] args)
        {
            DateTime updatedAt;
            int intervalInt = 108000000; // timer to run programm about 30 minuttes
            string connString = ConfigurationManager.AppSettings["connectionString"];
            dynamic json = fetchData();
            updatedAt = json.updatedAt.Value;

            DateTime lastDateTime = getLastDateTimeFromDB(json, connString); // get last datetime in database                                                                        
            Console.WriteLine("Last Datetime in database:  " + lastDateTime.ToString());

            foreach (var item in json.valutaKurser) // loop for printer json data 
            {
                printRateAndTime(updatedAt, Convert.ToDecimal(item.rate.Value), item.fromCurrency.Value, item.toCurrency.Value);
            }
            
            if (updatedAt.ToString() != lastDateTime.ToString()) // Here Checks at the time from json - url is not the same as Last datetime in database. if isn't the same, program should insert json in data base 

            {
                Console.WriteLine("the Time is not the same, \n Last Datetime in database:  " + lastDateTime.ToString() + "  " + "Datetime from json-url:  " + updatedAt.ToString());
                insertJsonDataInSQlEvent(json, connString);
                intervalInt = 108000000; // Set timer for 30 minuter for events igen
            }
            else // If there is the same time from json - url is and Last datetime in database. 
            {
                intervalInt = 60000; // timer for every minetes
            }

            // Timer for events
            Timer newTimer = new Timer();
            newTimer.Elapsed += new ElapsedEventHandler(fetchDataOnTimerEvent);
            newTimer.Interval = intervalInt; // inseter data every 30 minuter
            newTimer.Start();
            while (Console.Read() != 'q')
            {
                ;    // we can write anything here if we want, leaving this part blank won’t bother the code execution.
            }

        }
    }
}