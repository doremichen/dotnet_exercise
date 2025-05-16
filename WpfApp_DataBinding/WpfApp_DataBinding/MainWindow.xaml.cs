using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Windows;
using WpfApp_DataBinding.Models;


namespace WpfApp_DataBinding
{
    /// <summary>  
    /// Interaction logic for MainWindow.xaml  
    /// </summary>  
    public partial class MainWindow : Window
    {
        private Person _person;
        public MainWindow()
        {
            InitializeComponent();
            _person = new Person() { Name = "Initial", Comment = "Initial comment" };
            this.DataContext = _person;
        }

        // togole the name of the person
        private bool _isNameChanged = false;

        private void ChangeNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isNameChanged)
            {
                _person.Name = "Recover!!!";
            }
            else
            {
                _person.Name = "Changed!!!";
            }

            _isNameChanged = !_isNameChanged;

        }

        private void ChangeNameButton_Click1(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeNameButton_Click2(object sender, RoutedEventArgs e)
        {
            // show message box with the comment of person
            MessageBox.Show(_person.Comment);
        }

        private void OpenCollectionWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // open the collection window
            DemoCollectionWindow demoCollectionWindow = new DemoCollectionWindow();
            demoCollectionWindow.Show();
        }

        private void OpenConvertWindowButton_Click(object sender, RoutedEventArgs e)
        {
            // open the convert window
            DemoConvertWindow demoConvertWindow = new DemoConvertWindow();
            demoConvertWindow.Show();
        }
    }
}