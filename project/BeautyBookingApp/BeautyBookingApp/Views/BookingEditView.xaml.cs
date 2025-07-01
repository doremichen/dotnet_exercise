/**
 * Description: This file defines BookingEditView class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using BeautyBookingApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeautyBookingApp.Views
{
    /// <summary>
    /// Interaction logic for BookingEditView.xaml
    /// </summary>
    public partial class BookingEditView : UserControl
    {
        // Booking to edit
        private Booking _booking;

        // Action to notify when booking is saved
        public event Action? BookingUpdated;

        public Action? CloseAction { get; set; }

        public BookingEditView(Booking booking)
        {
            InitializeComponent();
            _booking = booking;

            // Initialize fields with booking data
            txtClientName.Text = booking.ClientName;
            datePicker.SelectedDate = booking.BookingTime.Date;
            timeBox.Text = booking.BookingTime.ToString("HH:mm");

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text)
            || datePicker.SelectedDate == null
            || !TimeSpan.TryParse(timeBox.Text, out TimeSpan time))
            {
                MessageBox.Show("請輸入正確的資訊");
                return;
            }

            var newDateTime = datePicker.SelectedDate.Value.Date + time;

            // copy data from _booking to orginalBooking
            if (newDateTime < DateTime.Now) {
                MessageBox.Show("預約時間不能在過去。");
                return;
            }
            var orginalBooking = new Booking
            {
                ClientName = _booking.ClientName,
                StaffUsername = _booking.StaffUsername,
                Service = _booking.Service,
                BookingTime = _booking.BookingTime
            };


            _booking.ClientName = txtClientName.Text;
            _booking.BookingTime = newDateTime;

            BookingService.UpdateBooking(_booking, orginalBooking);
            BookingUpdated?.Invoke();

            // close the edit view
            CloseAction?.Invoke();
        }
    }
}
