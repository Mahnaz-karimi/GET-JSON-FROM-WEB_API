using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using KmdWeb;

namespace UnitTestUpdating
{
    [TestClass]
    public class UnitTest1
    {
        double interval_event = 180000.0; // milliseconds for timer-events, database update time
        int reload_time_test = 3000; // time for reload data from sql database 2000 milisecend = 2 sec
        int day_ms_test = 86400000; // milisecends in day
        int hour_ms_test = 3600000; //milisecendes in hour
        dynamic json_test = currencies.json; // fetch json from the website
        DateTime update_At = json_test.updatedAt.Value;

        [TestMethod]
        public void TestMethod1()
        {
            var kmd = new KmdWeb();
            Double d = (DateTime.Now - update_At).TotalMilliseconds;
            Assert.AreEqual(kmd.TimerCalculategetDifference_Json_SQl(update_At), d);
        }
    }
}
