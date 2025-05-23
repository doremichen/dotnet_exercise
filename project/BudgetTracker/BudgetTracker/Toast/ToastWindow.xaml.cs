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

namespace BudgetTracker.Toast
{
    /// <summary>
    /// Interaction logic for ToastWindow.xaml
    /// </summary>
    public partial class ToastWindow : Window
    {
        public ToastWindow(string message, ToastType type, ToastPosition position)
        {
            InitializeComponent();
            DataContext = new ToastViewModel(message, type);
            Loaded += (_, _) => SetLocation(position);
        }

        private void SetLocation(ToastPosition pos)
        {
            Point pt = pos.GetPosition(Width, Height);
            Left = pt.X;
            Top = pt.Y;
        }
    }

    public class ToastViewModel
    {
        public string Message { get; }
        public Brush Background { get; }

        public ToastViewModel(string message, ToastType type)
        {
            Message = message;
            Background = type switch
            {
                ToastType.Success => Brushes.Green,
                ToastType.Warning => Brushes.Orange,
                ToastType.Error => Brushes.Red,
                _ => Brushes.SteelBlue
            };
        }
    }
}
