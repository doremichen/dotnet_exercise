/**
 * Description: This class is the audit logger service locator for the BeautyBookingApp project.
 * 
 * Author: Adam Chen
 * Date: 2025/08/04
 */
using BeautyBookingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Services
{
    public static class ServiceLocator
    {
        public static IAuditLogger? AuditLogger { get; set; }
    }
}
