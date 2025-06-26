using BudgetTracker.Data;
using BudgetTracker.Toast;
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

namespace BudgetTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // AppDbContext initialization
        private readonly AppDbContext _dbContext = DbContextFactory.Instance;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // create the database if it doesn't exist
            _dbContext.Database.EnsureCreated();

            ToastManager.Show("Welcome to Budget Tracker!", ToastType.Success, ToastPosition.TopRight);

        }

        override protected void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Dispose of the DbContext when the window is closing
            _dbContext.Dispose();
            base.OnClosing(e);
        }
    }
}