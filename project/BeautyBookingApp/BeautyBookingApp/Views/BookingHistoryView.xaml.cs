/**
 * Description: This file defines the user control: BookingHistoryView for the BeautyBookingApp project.
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
    /// Interaction logic for BookingHistoryView.xaml
    /// </summary>
    public partial class BookingHistoryView : UserControl
    {
        public event Action? BackToMenu;

        // private list _bookings, _username
        private List<Booking> _bookings = new List<Booking>();
        private string _username;

        // constructor with username parameter
        public BookingHistoryView(string username)
        {
            InitializeComponent();
            _username = username;
            LoadBookings();
        }

        private void LoadBookings()
        {
            _bookings = BookingService.GetUserBookings(_username);

            BookingListBox.ItemsSource = _bookings;


            if (_bookings.Count == 0)
            {
                BookingListBox.ItemsSource = null;
                BookingListBox.Items.Add("尚無預約紀錄。");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // back to main menu
            BackToMenu?.Invoke();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = BookingListBox.SelectedItems;
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("請先選取要刪除的預約紀錄。");
                return;
            }

            var result = MessageBox.Show($"確認刪除 {selectedItems.Count} 筆預約？", "刪除確認", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes)
                return;

            // 根據 index 找出要刪除的 bookings
            var toDelete = new List<Booking>();
            foreach (var item in selectedItems)
            {
                int index = BookingListBox.Items.IndexOf(item);
                if (index >= 0 && index < _bookings.Count)
                {
                    toDelete.Add(_bookings[index]);
                }
            }

            foreach (var b in toDelete)
            {
                BookingService.DeleteBooking(b);
            }

            MessageBox.Show("預約已成功刪除！");
            LoadBookings();
        }
    }
}
