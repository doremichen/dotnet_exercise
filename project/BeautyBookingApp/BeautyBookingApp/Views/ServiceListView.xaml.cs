/**
 * Descriptions: This code defines a ServiceListView class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using BeautyBookingApp.Services;
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
    /// Interaction logic for ServiceListView.xaml
    /// </summary>
    public partial class ServiceListView : UserControl
    {

        public event Action<ServiceItem>? ServiceSelected;

        public event Action? BackToMenu;

        public ServiceListView()
        {
            InitializeComponent();
            // Load the service items from the service data
            LoadServiceItems();
        }

        private void LoadServiceItems()
        {
            ServiceListBox.ItemsSource = ServiceData.GetServiceItems();
        }

        private void ServiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if an item is selected
            if (ServiceListBox.SelectedItem is ServiceItem selectedService)
            {
                // Raise the event to notify that a service has been selected
                ServiceSelected?.Invoke(selectedService);
            }

        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Raise the event to notify that the user wants to go back
            BackToMenu?.Invoke();
        }
    }
}
