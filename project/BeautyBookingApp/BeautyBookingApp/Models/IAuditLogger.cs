/**
 * Description: This interface defines a contract for an audit logger that can log user actions within the application.
 * 
 * Author: Adam Chen
 * Date: 2025/08/04
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Models
{
    public interface IAuditLogger
    {
        void Log(string user, string action, string target, string details = null);
    }
}
