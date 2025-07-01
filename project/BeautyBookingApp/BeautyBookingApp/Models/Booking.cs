/**
 * Descrption: This file defines the Booking class for the BeautyBookingApp.
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
    public class Booking
    {
        // StaffUsername, ClientName , ServiceItem, DateTime for the booking
        public string StaffUsername { get; set; } = String.Empty; // 登入者
        public string ClientName { get; set; } = string.Empty;
        public ServiceItem? Service { get; set; }
        public DateTime BookingTime { get; set; }

        public string DisplayTitle => $"{BookingTime:yyyy/MM/dd HH:mm} ｜ {Service?.Name}";
        public string DisplayDetail => $"價格：NT$ {Service?.Price}，用戶：{ClientName}";

        // toString override for displaying booking details
        public override string ToString()
        {
            return $"{BookingTime:yyyy/MM/dd HH:mm} - {Service?.Name} - {ClientName}";
        }

    }
}
