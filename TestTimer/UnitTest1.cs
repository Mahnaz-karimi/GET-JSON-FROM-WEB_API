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
        public void Test_Get_Diff_Json_Now()
        {                                 
            DateTime updateTimeTest = Convert.ToDateTime("2021-11-11T19:24:37.7190399Z");            
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest).TotalSeconds);
            Assert.AreEqual(Convert.ToInt64(TimerCalculate.getDiff_Json_Now(updateTimeTest)), Convert.ToInt64(d));            
        }


        [TestMethod]
        public void Test_Get_New_Timer0()
        {          
            DateTime updateTimeTest0 = Convert.ToDateTime("2021-11-5T19:24:37.7190399Z");
            Double d0 = Convert.ToDouble((DateTime.Now - updateTimeTest0).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d0), day_ms_test);         
        }

        [TestMethod]
        public void Test_Get_New_Timer1()
        {
            DateTime updateTimeTest1 = Convert.ToDateTime("2021-11-11T19:24:37.7190399Z");
            Double d1 = Convert.ToDouble((DateTime.Now - updateTimeTest1).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d1), reload_time_test + interval_event_test);            
        }

        [TestMethod]
        public void Test_Get_New_Timer3()
        {
            DateTime updateTimeTest2 = Convert.ToDateTime("2021-10-01T19:24:37.7190399Z");
            Double d2 = Convert.ToDouble((DateTime.Now - updateTimeTest2).TotalSeconds);
            Assert.AreEqual(TimerCalculate.get_New_Timer_Time(d2), day_ms_test);
        }

        [TestMethod]
        public void Test_Get_New_Timer4()
        {
            DateTime updateTimeTest2 = Convert.ToDateTime("2023-12-01T19:24:37.7190399Z");
            Double d = Convert.ToDouble((DateTime.Now - updateTimeTest2).TotalSeconds);
            Assert.IsTrue(TimerCalculate.get_New_Timer_Time(d)> day_ms_test);
        }

    }
    
}



