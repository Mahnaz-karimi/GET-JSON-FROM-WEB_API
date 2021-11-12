using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using KmdWeb;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http.Headers;

namespace TestTimer
{
    [TestClass]
    public class UnitTest1
    {
        double interval_event_test = 180.0; // milliseconds for timer-events, database update time
        int reload_time_test = 3; // time for reload data from sql database 2000 milisecend = 2 sec
        int day_ms_test = 86400; // milisecends in day
        int hour_ms_test = 3600; //milisecendes in hour

        [TestMethod]
        public void TestGetDifference()
        {
            dynamic jsonTest;
            using (WebClient wctest = new WebClient())
            {                
             jsonTest = JsonConvert.DeserializeObject(wctest.DownloadString("C:/Users/Mahnaz/Desktop/GET_JSON_FROM_WEB_API/TestTimer/currencies.json")); 
            }                      
            DateTime updateTimeTest = jsonTest.updatedAt.Value;            
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(Convert.ToInt64(TimerCalculate.getDifference_Json_Now(updateTimeTest)), Convert.ToInt64(d));
            
        }
        [TestMethod]
        public void TestIntervalInt_when_json_is_On_time()
        {
            dynamic jsonTest;
            using (WebClient wctest = new WebClient())
            {
                jsonTest = JsonConvert.DeserializeObject(wctest.DownloadString("C:/Users/Mahnaz/Desktop/GET_JSON_FROM_WEB_API/TestTimer/currencies.json"));
            }
            DateTime updateTimeTest = jsonTest.updatedAt.Value;
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_new_IntervalInt_when_json_is_On_time(d), interval_event_test-d + reload_time_test);
        }
        [TestMethod]
        public void Test_Get_New_Timer_Time()
        {
            dynamic jsonTest;
            using (WebClient wctest = new WebClient())
            {
                jsonTest = JsonConvert.DeserializeObject(wctest.DownloadString("C:/Users/Mahnaz/Desktop/GET_JSON_FROM_WEB_API/TestTimer/currencies.json"));
            }
            DateTime updateTimeTest = jsonTest.updatedAt.Value;
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(System.Math.Abs(d)), reload_time_test);
        }
        
    }
    
}



