/**
 * Description: This class represents a student model
 * Author: Adam Chen
 * Date: 2025-06-26
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StudentGradeManager.Models
{
    public class Student : INotifyPropertyChanged
    {
        [Key]
        public int StudentId { get; set; }
        // name of the student
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        // class name of the student
        private string _className = string.Empty;
        public string ClassName
        {
            get => _className;
            set { _className = value; OnPropertyChanged(nameof(ClassName)); }
        }

        // readonly total score of the student
        public double TotalScore
        {
            get
            {
                return Grades.Sum(g => g.Score);
            }
        }
        // readonly average score of the student
        public double AverageScore
        {
            get
            {
                return Grades.Count > 0 ? Math.Round(Grades.Average(g => g.Score), 2) : 0;
            }
        }


        // list of grades for the student
        public ICollection<Grade> Grades { get; set; } = new List<Grade>();

        // RaiseScoreChanged
        public void RaiseScoreChanged()
        {
            OnPropertyChanged(nameof(TotalScore));
            OnPropertyChanged(nameof(AverageScore));
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
