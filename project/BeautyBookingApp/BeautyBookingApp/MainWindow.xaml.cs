using BeautyBookingApp.Models;
using BeautyBookingApp.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BeautyBookingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // show login view on startup
            ShowLogin();
        }

        private void ShowLogin()
        {
            // Create and show the login view
            Views.LoginView loginView = new Views.LoginView();
            loginView.LoginSucceeded += OnLoginSucceeded;

            MainContainer.Children.Clear();
            MainContainer.Children.Add(loginView);
        }

        private void OnLoginSucceeded(string username)
        {
            ShowMainMenu(username);
        }

        private string _currentUsername = "";
        private void ShowMainMenu(string username)
        {
            _currentUsername = username;

            var stack = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var welcome = new TextBlock
            {
                Text = $"歡迎登入，{username}",
                FontSize = 20,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var btnServices = new Button
            {
                Content = "預約服務",
                Width = 200,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 10)
            };
            btnServices.Click += (s, e) => ShowServiceList();

            var btnHistory = new Button
            {
                Content = "查看預約紀錄",
                Width = 200,
                Height = 40
            };
            btnHistory.Click += (s, e) => ShowBookingHistory();

            // exit button
            var btnExit = new Button
            {
                Content = "登出",
                Width = 200,
                Height = 40,
                Margin = new Thickness(0, 10, 0, 0)
            };
            btnExit.Click += (s, e) =>
            {
                // 清除當前用戶名並返回登入畫面
                _currentUsername = string.Empty;
                ShowLogin();
            };

            stack.Children.Add(welcome);
            stack.Children.Add(btnServices);
            stack.Children.Add(btnHistory);
            stack.Children.Add(btnExit);

            MainContainer.Children.Clear();
            MainContainer.Children.Add(stack);
        }

        private void ShowBookingHistory()
        {
            var view = new Views.BookingHistoryView(_currentUsername);
            view.BackToMenu += () => ShowMainMenu(_currentUsername);
            MainContainer.Children.Clear();
            MainContainer.Children.Add(view);
        }

        private void ShowServiceList()
        {
            var serviceListView = new Views.ServiceListView();
            serviceListView.ServiceSelected += service =>
            {
                ShowBookingView(service);
            };

            MainContainer.Children.Clear();
            MainContainer.Children.Add(serviceListView);
        }

        private void ShowBookingView(ServiceItem service)
        {
            var bookingView = new BookingView(service, _currentUsername);
            bookingView.BookingCompleted += () =>
            {
                // 回到主選單
                ShowMainMenu(_currentUsername);
            };

            MainContainer.Children.Clear();
            MainContainer.Children.Add(bookingView);
        }

        
    }
}