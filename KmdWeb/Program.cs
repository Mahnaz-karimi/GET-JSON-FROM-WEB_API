
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
            System.Console.WriteLine(string.Format("{0:0.0000}", rate) + " Date:  " + dt.ToString());
        }

        public static void fetchDataTimerEvent(object source, ElapsedEventArgs e)
        {
            dynamic json = fetchData();
            if (json.updatedAt.Value.ToString() != getLastDateTimeFromSQLDB( json, ConfigurationManager.AppSettings["connectionString"]).ToString() )// If statement. Check updatedAt in json is the same Last datetime in database, if it is so don't do eny things 
            {
                insertJsonDataInSQlEvent(json, ConfigurationManager.AppSettings["connectionString"]);
            }
        }

        public static DateTime getLastDateTimeFromSQLDB(dynamic json, string connString)
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
                    Console.WriteLine("Last Datetime in database:  " + dtLine.ToString() );
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
                cmd.CommandType = CommandType.Text;
                cmd.Connection = conn;
                foreach (var item in json.valutaKurser)
                {
                    Console.WriteLine("RATE: " + Convert.ToString(item.rate.Value));
                    cmd.CommandText = "INSERT INTO ValutaKurser (FromCurrency, ToCurrency, Rate, UpdatedAt )";
                    cmd.CommandText += "Values ('" + item.fromCurrency.Value + "', '" + item.toCurrency.Value + "', CAST(" + Convert.ToString(item.rate.Value).Replace(',','.') + " AS NUMERIC(25,15)), convert(datetime2,'" + json.updatedAt.Value.ToString() + "',103))";

                    cmd.Connection = conn;
                    cmd.ExecuteNonQuery();
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine("erorr is ", ex.Message);
                    }

                }                
            }

        }

        public static void Main(string[] args)
        {
            DateTime updatedAt;
            int intervalInt = 108000000; // timer to run programm about 30 minuttes
            string connString = ConfigurationManager.AppSettings["connectionString"];
            dynamic json = fetchData();
            updatedAt = json.updatedAt.Value;
            Console.WriteLine("updatedAT:  " + updatedAt.ToString());

            DateTime lastDateTime = getLastDateTimeFromSQLDB(json, connString); // get last datetime in database                                                                        
            Timer newTimer = new Timer();

            foreach (var item in json.valutaKurser) // loop for printer json data 
            {
                printRateAndTime(updatedAt, Convert.ToDecimal(item.rate.Value), item.fromCurrency.Value, item.toCurrency.Value);
            }            

            if (updatedAt.ToString() != lastDateTime.ToString()) // If statement. Check updatedAt in json is the same Last datetime in database, if it is so don't do eny things 
            {
                Console.WriteLine("the Time is not the same, /n Last Datetime in database:  " + lastDateTime.ToString() + "  " + "updatedAT:  " + updatedAt.ToString());
                insertJsonDataInSQlEvent(json, connString);
                intervalInt = 108000000;
            }
            else
            {
                intervalInt = 60000; // timer for every minetes
            }           
           
           // Timer for events
            newTimer.Elapsed += new ElapsedEventHandler(fetchDataTimerEvent);
            newTimer.Interval = intervalInt; // inseter data every 30 minuter
            newTimer.Start();
            while (Console.Read() != 'q')
            {
                ;    // we can write anything here if we want, leaving this part blank won’t bother the code execution.
            }

        }
    }
}