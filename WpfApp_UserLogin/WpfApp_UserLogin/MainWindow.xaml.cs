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
using WpfApp_UserLogin.ViewModels;

namespace WpfApp_UserLogin
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Set the DataContext to the ViewModel 
            this.DataContext = new ViewModels.LogInViewModel();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LogInViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password; // Get the password from the PasswordBox
                // Call the LogIn method from the ViewModel
                viewModel.LogIn();
                // show LoginViewModel information in MessageBox
                var message = viewModel.Username + " " + viewModel.Password + " ";
                showMessage(message);

            }
        }

        // Show message in MessageBox for usermodel
        private void showMessage(string message)
        { 
            MessageBox.Show(message, "Login Status", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}