/**
 * Description: This class is the CourseSelectDialog, which is a dialog for selecting a course from a list of subjects.
 * 
 * Author: Adam chen
 * Date: 2025/08/04
 */
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
using TimetableGenerator.Models;

namespace TimetableGenerator.Views
{
    /// <summary>
    /// Interaction logic for CourseSelectDialog.xaml
    /// </summary>
    public partial class CourseSelectDialog : Window
    {

        // selected subject
        public Subject? SelectedSubject { get; private set; }

        public CourseSelectDialog(List<Subject>? subjects)
        {
            InitializeComponent();
            // Set the DataContext to the list of subjects for binding
            CourseListBox.ItemsSource = subjects;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            SelectedSubject = CourseListBox.SelectedItem as Subject;
            // Check if a subject is selected
            if (SelectedSubject == null)
            {
                MessageBox.Show("請選擇一個科目。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true; // Set the dialog result to true
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Set the dialog result to false
        }
    }
}
