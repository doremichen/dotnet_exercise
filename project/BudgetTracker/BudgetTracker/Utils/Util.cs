using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BudgetTracker.Utils
{
    class Util
    {
        // show log with timestamp
        public static void Log(string message)
        {
            Debug.WriteLine($"[{DateTime.Now}] {message}");
        }

    }
}
