using System.Windows;

namespace WpfApp_DataBinding
{
    /// <summary>
    /// Interaction logic for DemoConvertWindow.xaml
    /// </summary>
    public partial class DemoConvertWindow : Window
    {
        public DemoConvertWindow()
        {
            InitializeComponent();
            // Set the DataContext for the window to the ConvertViewModel
            this.DataContext = new ViewModels.ConvertViewModel();
        }
    }
}
