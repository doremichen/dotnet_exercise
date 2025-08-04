/**
 * Description: This file defines BookingService class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using BeautyBookingApp.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Services
{
    public static class BookingService
    {
        // filepath: "bookings.json"
        private static readonly string BookingFilePath = AppDataHelper.GetAppDataFilePath("bookings.json");

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
                .Where(b => b.StaffUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                .OrderBy(b => b.BookingTime)
                .ToList();
        }

        // delete a booking by username and booking time
        public static void DeleteBooking(Booking bookingToDelete)
        {
            var bookings = LoadBookings();

            // 使用 Booking 的 ClientName + BookingTime + Service 名稱 比對（簡單唯一鍵）
            var target = bookings.FirstOrDefault(b =>
                b.ClientName == bookingToDelete.ClientName &&
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

        internal static void UpdateBooking(Booking updated, Booking org)
        {
            var bookings = LoadBookings();

            DumpBookingsToConsole();

            // log updated and org booking information
            Debug.WriteLine($"Updating booking: {updated} from original: {org}");
            // 根據時間與原始服務項目與客戶名搜尋原始資料（簡易匹配）
            var index = bookings.FindIndex(b =>
                b.BookingTime == org.BookingTime &&
                b.Service?.Name == org.Service?.Name &&
                b.ClientName == org.ClientName);
            Debug.WriteLine($"index: {index}");
            if (index >= 0)
            {
                bookings[index] = updated;
            }
            else
            {
                // 或者找不到原本的就直接覆蓋新增也可
                bookings.Add(updated);
            }

            var json = JsonConvert.SerializeObject(bookings, Formatting.Indented);
            File.WriteAllText(BookingFilePath, json);
        }

        /**
         * dump all element in bookings to console for debugging
         */
        public static void DumpBookingsToConsole()
        {
            var bookings = LoadBookings();
            foreach (var booking in bookings)
            {
                Debug.WriteLine(booking);
            }
        }

        /**
         * get all upcoming bookings after the given dateTime
         */
        internal static List<Models.Booking> GetUpcomingBookings(DateTime dateTime)
        {

            DumpBookingsToConsole();

            // get all bookings that are after the given dateTime
            var bookings = LoadBookings();
            return bookings
                .Where(b => b.BookingTime.Date == dateTime.Date)
                .OrderBy(b => b.BookingTime)
                .ToList();
        }
    }
}
