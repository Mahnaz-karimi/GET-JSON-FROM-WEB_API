using System;
using System.Net;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Dynamic;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace KmdWeb

{    public class DataHandling
    {
        public static dynamic fetchData() // get json data 
        {
            dynamic json = new ExpandoObject();
            using (WebClient wc = new WebClient())
            {
                json = JsonConvert.DeserializeObject(wc.DownloadString("http://127.0.0.1:8000/"));               
            }
            return json ;
        }

        public static DateTime getLastDateTimeFromDB(string connString)
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
        public static void insertJsonDataInSQl(dynamic json, string connString, string ValutaKurser)
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection conn = new SqlConnection(connString)) // save json-data to sql database
            {
                conn.Open();
                cmd.Connection = conn;
                foreach (var item in json.valutaKurser)
                {
                    Console.WriteLine("INSERTING DATA IN SQL..... Rate is: " + Convert.ToString(item.rate.Value) + " updateAt is " + Convert.ToString(json.updatedAt.Value));
                    cmd.CommandText = "INSERT INTO "+ ValutaKurser +" (FromCurrency, ToCurrency, Rate, UpdatedAt)";
                    cmd.CommandText += "Values ('" + item.fromCurrency.Value + "', '" + item.toCurrency.Value + "', CAST(" + Convert.ToString(item.rate.Value).Replace(',', '.') + " AS NUMERIC(25,15))," +
                                        "convert(datetime2,'" + json.updatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff") + "'))";
                    cmd.ExecuteNonQuery();
                }                
            }
        }
        
        public static void print_Json(dynamic json, DateTime updatedAt)
        {
            foreach (var item in json.valutaKurser) // print json-data 
            {
                Console.WriteLine("DATA FROM JSON: Rate:  " + string.Format("{0:0.0000000000000000}", (item.rate.Value) +
                    "   fromCurrency: " + item.fromCurrency.Value + "   toCurrency: " + item.toCurrency.Value));
            }
            Console.WriteLine("updatedAt:  " + (updatedAt.ToString("yyyy-MM-dd HH:mm:ss.fffffff \n")));
        }
    }
}
