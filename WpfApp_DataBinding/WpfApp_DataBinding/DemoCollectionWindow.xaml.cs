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
using WpfApp_DataBinding.ViewModels;

namespace WpfApp_DataBinding
{
    /// <summary>
    /// Interaction logic for DemoCollectionWindow.xaml
    /// </summary>
    public partial class DemoCollectionWindow : Window
    {
        public DemoCollectionWindow()
        {
            InitializeComponent();
            this.DataContext = new CollectionViewModel();
        }

        private void AddPersonButton_Click(object sender, RoutedEventArgs e)
        {
            // Add a new person to the collection
            var viewModel = this.DataContext as CollectionViewModel;
            if (viewModel != null)
            {
                viewModel.People.Add(new Models.Person() { Name = "New Person", Comment = "New comment" });
            }
        }

        private void DeletePersonButton_Click(object sender, RoutedEventArgs e)
        {
            // delete the selected person from the collection
            var viewModel = this.DataContext as CollectionViewModel;
            if (viewModel != null)
            {
                var selectedPerson = PersonListBox.SelectedItem as Models.Person;
                if (selectedPerson != null)
                {
                    viewModel.People.Remove(selectedPerson);
                }
            }
        }
    }
}
