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
using System.Windows.Shapes;

namespace BeautyBookingApp.Views
{
    /// <summary>
    /// Interaction logic for BookingView.xaml
    /// </summary>
    public partial class BookingView : UserControl
    {
        private ServiceItem? _service;
        private string _username;

        public event Action? BookingCompleted;

        public BookingView(ServiceItem service, string username)
        {
            InitializeComponent();
            _service = service;
            _username = username;

            txtServiceName.Text = service.Name;
            txtDescription.Text = service.Description;
            txtPrice.Text = $"價格：NT$ {service.Price}";
            txtDuration.Text = $"所需時間：約 {service.DurationMinutes} 分鐘";
        }

        private void ConfirmBooking_Click(object sender, RoutedEventArgs e)
        {
            if (datePicker.SelectedDate == null || string.IsNullOrWhiteSpace(timeInput.Text))
            {
                MessageBox.Show("請選擇日期並輸入時間！");
                return;
            }

            if (!TimeSpan.TryParse(timeInput.Text, out TimeSpan selectedTime))
            {
                MessageBox.Show("時間格式錯誤，請使用 HH:mm");
                return;
            }

            var bookingTime = datePicker.SelectedDate.Value.Date + selectedTime;

            // save booking information by BookingService
            BookingService.SaveBooking(new Booking
            {
                StaffUsername = _username,
                ClientName = clientNameInput.Text.Trim(),
                Service = _service,
                BookingTime = bookingTime
            });

            // 這裡可進一步保存資料，目前先模擬顯示
            MessageBox.Show($"預約成功：{_service!.Name}\n時間：{bookingTime}");

            //通知主畫面回到主選單
            BookingCompleted?.Invoke();

        }

        private void CancelBooking_Click(object sender, RoutedEventArgs e)
        {
            // 取消預約，回到主選單
            BookingCompleted?.Invoke();
        }
    }
}
