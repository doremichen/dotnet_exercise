/**
 * FileComparerApp.Utils.Util
 * This class is part of the FileComparerApp project.
 * It contains utility methods for the application.
 * 
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.Utils
{
    public static class Util
    {

        /**
         * Log - Logs a message to the console.
         * 
         * param message - The message to log.
         * returns - void
         */
        public static void Log(string message)
        {
            Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
        }

    }
}
