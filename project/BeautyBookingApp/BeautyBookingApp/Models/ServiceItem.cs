/**
 * Description: This file defines ServiceItem class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Models
{
    public class ServiceItem
    {
        // Name, Decripttion, Price, DurationMinutes for the service item
        public string Name { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

    }
}
