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
using CppCliWrapper; // Assuming this is the namespace of your C++/CLI wrapper

namespace WpfAppCSharp
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

        private void AddNumbers_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtNum1.Text, out int num1) && int.TryParse(txtNum2.Text, out int num2))
            {
                // 呼叫 C++/CLI 封裝的靜態函式，該函式內部再呼叫原生 C++ 函式
                int sum = NativeCalculator.Add(num1, num2);
                lblResult.Text = sum.ToString();
            }
            else
            {
                lblResult.Text = "請輸入有效的數字！";
            }

        }

        private void GetGreeting_Click(object sender, RoutedEventArgs e)
        {
            // 呼叫 C++/CLI 封裝的靜態函式，該函式內部再呼叫原生 C++ 函式
            string greeting = NativeCalculator.GetGreeting();
            lblGreeting.Text = greeting;
        }
    }
}