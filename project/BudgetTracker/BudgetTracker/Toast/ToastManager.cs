using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BudgetTracker.Toast
{
    public static class ToastManager
    {
        // static show method to show toast
        public static void Show(string message, ToastType type = ToastType.Info, ToastPosition? position = null, int durationMs = 2000)
        {
            // Dispatcher check to ensure the toast is shown on the UI thread
            if (Application.Current.Dispatcher.CheckAccess())
            {
                var toast = new ToastWindow(message, type, position ?? ToastPosition.BottomRight);
                toast.Show();

                // Set the toast to close after 3 seconds
                Task.Delay(durationMs).ContinueWith(_ =>
                {
                    if (toast.IsVisible)
                    {
                        Application.Current.Dispatcher.Invoke(() => toast.Close());
                    }
                });

            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => Show(message, type, position));
            }
        }
    }
}
