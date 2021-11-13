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
        int hour_ms_test = 3600; //milisecendes in hour
        int day_ms_test = 86400; // milisecends in day

        [TestMethod]
        public void Test_Get_Diff_Json_Now()
        {                                 
            DateTime updateTimeTest = Convert.ToDateTime("2021-11-11T19:24:37.7190399Z");            
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(Convert.ToInt64(TimerCalculate.getDiff_Json_Now(updateTimeTest)), Convert.ToInt64(d));            
        }
        public enum TimeComparison
        {
            EarlierThan = -1,
            TheSameAs = 0,
            LaterThan = 1
        }

        [TestMethod]
        public void Test_Get_New_Timer_0() // a days ago
        {

            DateTime localTime = DateTime.Now;
            DateTime utcTime = DateTime.UtcNow;

            Console.WriteLine("Difference between {0} and {1} time: {2}:{3} hours",
                              localTime,
                              utcTime,
                              (localTime - utcTime).Hours,
                              (localTime - utcTime).Minutes);
            Console.WriteLine("The {0} time is {1} the {2} time.",
                              localTime.Kind,
                              Enum.GetName(typeof(TimeComparison), localTime.CompareTo(utcTime)),
                              utcTime.Kind);
            
            Double d = Convert.ToDouble((DateTime.Now - utcTime).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d), reload_time_test + interval_event_test);         
        }

        [TestMethod]
        public void Test_Get_New_Timer1() // it works 
        {
            DateTime updateTimeTest = Convert.ToDateTime("2021-11-10T19:24:37.7190399Z");
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d), hour_ms_test);            
        }

        [TestMethod]
        public void Test_Get_New_Timer3() 
        {
            //date time now is 13.11.21 
            DateTime updateTimeTest = Convert.ToDateTime("2021-10-01T19:24:37.7190399Z");
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d), day_ms_test);
        }

        [TestMethod]
        public void Test_Get_New_Timer4() 
        {
            //date time now is 13.11.21 
            DateTime updateTimeTest = Convert.ToDateTime("2023-12-01T19:24:37.7190399Z");
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Console.WriteLine("should be minus");
            Assert.IsTrue(TimerCalculate.get_New_Timer_Time(d)> hour_ms_test);
        }

    }
    
}



