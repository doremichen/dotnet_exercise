/**
 * Description: This file defines BookingService class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Services
{
    public static class BookingService
    {
        // filepath: "bookings.json"
        private const string BookingFilePath = "bookings.json";

        // load bookings from JSON file
        // return list of bookings
        public static List<Models.Booking> LoadBookings()
        {
            if (!File.Exists(BookingFilePath))
                return new List<Booking>();

            string json = File.ReadAllText(BookingFilePath);
            return JsonConvert.DeserializeObject<List<Booking>>(json) ?? new List<Booking>();
        }

        // save booking to JSON file and add to the list bookings
        public static void SaveBooking(Models.Booking booking)
        {
            var bookings = LoadBookings();
            bookings.Add(booking);

            string json = JsonConvert.SerializeObject(bookings, Formatting.Indented);
            File.WriteAllText(BookingFilePath, json);
        }

        // get all bookings for a specific user
        public static List<Models.Booking> GetUserBookings(string username)
        {
            var bookings = LoadBookings();
            return bookings
                .Where(b => b.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.BookingTime)
                .ToList();
        }

        // delete a booking by username and booking time
        public static void DeleteBooking(Booking bookingToDelete)
        {
            var bookings = LoadBookings();

            // 使用 Booking 的 Username + BookingTime + Service 名稱 比對（簡單唯一鍵）
            var target = bookings.FirstOrDefault(b =>
                b.Username == bookingToDelete.Username &&
                b.BookingTime == bookingToDelete.BookingTime &&
                b.Service?.Name == bookingToDelete.Service?.Name
            );

            if (target != null)
            {
                bookings.Remove(target);
                string json = JsonConvert.SerializeObject(bookings, Formatting.Indented);
                File.WriteAllText(BookingFilePath, json);
            }
        }

    }
}
