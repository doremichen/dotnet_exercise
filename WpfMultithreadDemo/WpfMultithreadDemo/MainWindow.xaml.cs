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

namespace WpfMultithreadDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // disable the button to prevent multiple clicks
            btnStart.IsEnabled = false;
            // show text status is starting
            txtStatus.Text = "Starting...";
            // progressbar value to 0
            progressBar.Value = 0;

            // create a new thread to perform the long-running task
            Task.Run(() =>
            {
                // simulate a long-running task
                for (int i = 0; i <= 100; i++)
                {
                    // update the progress bar value
                    Thread.Sleep(50); // simulate work
                    UpdateProgress(i);
                }
            }).ContinueWith(t =>            // continue after the task is done, it can use asyn/await as well
            {
                // update UI after task completion
                txtStatus.Text = "Completed!";
                btnStart.IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void UpdateProgress(int i)
        {
            // update the progress bar value on the UI thread
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = i;
                txtStatus.Text = $"Progress: {i}%";
            });
        }
    }
}