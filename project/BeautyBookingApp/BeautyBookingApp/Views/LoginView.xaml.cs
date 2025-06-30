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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        // event login succedsfully
        public event Action<string>? LoginSucceeded;

        public LoginView()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            // username and password validation logic
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Password.Trim();

            // test password for demo purposes: 1234
            if (username == "admin" && password == "1234")
            {
                // raise the event to notify that login succeeded
                LoginSucceeded?.Invoke(username);
            }
            else
            {
                MessageBox.Show("無效帳號或密碼", "登入失敗", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
