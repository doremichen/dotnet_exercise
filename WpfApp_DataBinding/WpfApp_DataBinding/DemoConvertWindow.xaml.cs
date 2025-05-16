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
