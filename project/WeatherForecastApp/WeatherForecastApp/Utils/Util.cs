using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherForecastApp.Utils
{
    public static class Util
    {
        // log with timestamp
        public static void Log(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Debug.WriteLine($"[{timestamp}] {message}");
        }

        // log with timestamp and thread id
        public static void LogWithThread(string message)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var threadId = Environment.CurrentManagedThreadId;
            Debug.WriteLine($"[{timestamp}] [Thread {threadId}] {message}");
        }
    }
}
