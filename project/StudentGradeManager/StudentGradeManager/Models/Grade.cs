/**
 * Description: This class represents a Grade model
 * Author: Adam Chen
 * Date: 2025-06-26
 * 
 */
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Formats.Asn1.AsnWriter;


namespace StudentGradeManager.Models
{
    public class Grade : INotifyPropertyChanged
    {
        [Key]
        public int GradeId { get; set; }
        // subject of the grade
        private string _subject = string.Empty;
        public string Subject
        {
            get => _subject;
            set { _subject = value; OnPropertyChanged(nameof(Subject)); }
        }
        // score of the grade
        private double _score;
        public double Score
        {
            get => _score;
            set { _score = value; OnPropertyChanged(nameof(Score)); }
        }

        // studentId of the student who received the grade
        public int StudentId { get; set; }
        // Student who received the grade
        public Student Student { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}