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

namespace StudentGradeManager.Views
{
    /// <summary>
    /// Interaction logic for AddGradeWindowxaml.xaml
    /// </summary>
    public partial class AddGradeWindowxaml : Window
    {
        // public subject and score properties for the grade
        public string Subject { get; private set; } = "";
        public double Score { get; private set; }


        public AddGradeWindowxaml(string subject = "", double score = 0)
        {
            InitializeComponent();
            // Initialize the TextBoxes with the provided subject and score
            SubjectTextBox.Text = subject;
            ScoreTextBox.Text = score.ToString("F2"); // format score to 2 decimal places
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            // subject from TextBox
            Subject = SubjectTextBox.Text.Trim();
            // score from TextBox, parse to double
            if (double.TryParse(ScoreTextBox.Text.Trim(), out double score))
            {
                Score = score;
                DialogResult = true; // set dialog result to true
                Close(); // close the window
            }
            else
            {
                MessageBox.Show("Please enter a valid score.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
