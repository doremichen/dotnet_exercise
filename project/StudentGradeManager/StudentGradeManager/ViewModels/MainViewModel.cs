/**
 * Description: This class is a MainViewModel for the Student Grade Manager application
 * Author: Adam Chen
 * Date: 2025-06-26
 */
using LiveCharts;
using LiveCharts.Wpf;
using StudentGradeManager.Commands;
using StudentGradeManager.Data;
using StudentGradeManager.Models;
using StudentGradeManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StudentGradeManager.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Observable collection of students
        public ObservableCollection<Student> Students { get; set; } = new ObservableCollection<Student>();
        // Observable collection of grades
        public ObservableCollection<Grade> Grades { get; set; } = new ObservableCollection<Grade>();


        // public addStudent ICommand
        public ICommand AddStudentCommand { get; }
        // public removeStudent ICommand
        public ICommand RemoveStudentCommand { get; }
        // Save changes ICommand
        public ICommand SaveStudentCommand { get; }
        // Add Grade ICommand
        public ICommand AddGradeCommand { get; }
        // Edit Grade ICommand
        public ICommand EditGradeCommand { get; }
        // Remove Grade ICommand
        public ICommand RemoveGradeCommand { get; }
        // show statistics ICommand
        public ICommand ShowStatisticsCommand { get; }
        // export CSV ICommand
        public ICommand ExportCsvCommand { get; }


        // AppDbContext for database operations
        private AppDbContext _dbContext = new AppDbContext();

        // selected grade property
        private Grade? _selectedGrade;
        public Grade? SelectedGrade
        {
            get => _selectedGrade;
            set
            {
                if (_selectedGrade != value)
                {
                    _selectedGrade = value;
                    OnPropertyChanged(nameof(SelectedGrade));
                }
            }
        }


        // selected student property
        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (_selectedStudent != value)
                {
                    _selectedStudent = value;
                    OnPropertyChanged(nameof(SelectedStudent));
                    LoadGrades(); // load grades when a student is selected
                }
            }
        }

        private void LoadGrades()
        {
            // clear the existing grades collection
            Grades.Clear();
            // check if a student is selected
            if (SelectedStudent != null)
            {
                var grades = _dbContext.Grades.Where(g => g.StudentId == SelectedStudent.StudentId).ToList();
                foreach (var g in grades)
                    Grades.Add(g);

                // update the selected student's total score and average score
                SelectedStudent.RaiseScoreChanged();
            }

            // logging the loading of grades for the selected student
            Debug.WriteLine($"Grades loaded for student: {SelectedStudent?.Name}");

            
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public MainViewModel() 
        {
            // logging the initialization of MainViewModel
            Debug.WriteLine("MainViewModel initialized.");

            // create database and Load students from the database
            _dbContext.Database.EnsureCreated();
            var studentsFromDb = _dbContext.Students.ToList();
            foreach (var student in studentsFromDb)
            {
                // logging each student loaded from the database
                Debug.WriteLine($"Loaded student: {student.Name}");
                Students.Add(student);
            }

            // Initialize commands
            AddStudentCommand = new RelayCommand(_ => AddStudent());
            RemoveStudentCommand = new RelayCommand(_ => RemoveStudent(), _ => SelectedStudent != null);
            SaveStudentCommand = new RelayCommand(_ => SaveStudent(), _ => SelectedStudent != null);
            AddGradeCommand = new RelayCommand(_ => OpenGradeDialog(), _ => SelectedStudent != null);
            EditGradeCommand = new RelayCommand(_ => EditGrade(), _ => SelectedGrade != null); 
            RemoveGradeCommand = new RelayCommand(_ => RemoveGrade(), _ => SelectedGrade != null);
            ShowStatisticsCommand = new RelayCommand(_ => ShowStatistics(), _ => SelectedStudent != null);
            ExportCsvCommand = new RelayCommand(_ => ExportToCsv(), _ => Students.Count > 0);
        }

        private void ExportToCsv()
        {
            // new save file dialog to export CSV
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "StudentGrades.csv",
                DefaultExt = ".csv",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*"
            };

            // show the save file dialog and check if the user clicked OK
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // create a StringBuilder to build the CSV content
                    var csvContent = new StringBuilder();
                    // add header row
                    csvContent.AppendLine("學生ID,姓名,班級,科目,分數");
                    // iterate through each student and their grades
                    foreach (var student in Students)
                    {
                        foreach (var grade in student.Grades)
                        {
                            csvContent.AppendLine($"{student.StudentId},{student.Name},{student.ClassName},{grade.Subject},{grade.Score}");
                        }
                    }
                    // write the CSV content to the selected file
                    System.IO.File.WriteAllText(saveFileDialog.FileName, csvContent.ToString(), new UTF8Encoding(true));
                    Debug.WriteLine($"CSV exported successfully to {saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error exporting CSV: {ex.Message}");
                }
            }
        }

        private void ShowStatistics()
        {
            var studentsCopy = Students.Select(s => new
            {
                s.Name,
                Avg = s.AverageScore,
                Max = s.Grades.Count > 0 ? s.Grades.Max(g => g.Score) : 0,
                Min = s.Grades.Count > 0 ? s.Grades.Min(g => g.Score) : 0,
            }).ToList();

            // statisticsViewModel to show statistics
            var viewModel = new StatisticsViewModel(studentsCopy.Select(s => s.Name).ToList())
            {
                AverageSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "平均分數",
                        Values = new ChartValues<double>(studentsCopy.Select(s => s.Avg))
                    }
                },
                
                MaxSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "最高分",
                        Values = new ChartValues<double>(studentsCopy.Select(s => s.Max))
                    }
                },
                MinSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "最低分",
                        Values = new ChartValues<double>(studentsCopy.Select(s => s.Min))
                    }
                }
            };

            // show statistics window
            var statisticsWindow = new Views.StutentsStaticsChart();
            statisticsWindow.DataContext = viewModel;
            // show the statistics window as a dialog
            statisticsWindow.ShowDialog();

        }

        private void RemoveGrade()
        {
            // check if a grade is selected
            if (SelectedGrade != null)
            {
               // find the existing grade in the database
                var existingGrade = _dbContext.Grades.Find(SelectedGrade.GradeId);
                if (existingGrade != null)
                {
                    // remove the selected grade from the database
                    _dbContext.Grades.Remove(existingGrade);
                    try
                    {
                        _dbContext.SaveChanges(); // save changes to the database
                        Debug.WriteLine($"Grade removed: {SelectedGrade.Subject} - {SelectedGrade.Score}");
                        // remove the selected grade from the Grades collection for UI binding
                        Grades.Remove(SelectedGrade);
                        SelectedGrade = null; // clear selection after removal

                        // update the selected student's total score and average score
                        SelectedStudent?.RaiseScoreChanged();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error removing grade: {ex.Message}");
                    }
                }
                else
                {
                    // if not found, log an error
                    Debug.WriteLine("Selected grade not found in the database.");
                }
            }

        }

        private void EditGrade()
        {
            // dialog to edit the selected grade
            var dialog = new Views.AddGradeWindowxaml(SelectedGrade.Subject, SelectedGrade.Score);
            // check if a grade is selected before opening the dialog
            if (dialog.ShowDialog() == true && SelectedGrade != null)
            {
                // find the existing grade in the database
                var existingGrade = _dbContext.Grades.Find(SelectedGrade.GradeId);
                if (existingGrade != null)
                {
                    // update the existing grade's properties
                    existingGrade.Subject = dialog.Subject;
                    existingGrade.Score = dialog.Score;
                }
                else
                {
                    // if not found, log an error
                    Debug.WriteLine("Selected grade not found in the database.");
                }

                try
                {
                    _dbContext.SaveChanges(); // save changes to the database
                    Debug.WriteLine($"Grade updated: {SelectedGrade.Subject} - {SelectedGrade.Score}");
                    // update the grade in the Grades collection for UI binding
                    SelectedGrade.Subject = dialog.Subject;
                    SelectedGrade.Score = dialog.Score;

                    // update the selected student's total score and average score
                    SelectedStudent?.RaiseScoreChanged();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating grade: {ex.Message}");
                }
            }
            else
            {
                // logging the cancellation of the dialog
                Debug.WriteLine("Edit Grade dialog was cancelled.");
            }


        }

        private void OpenGradeDialog()
        {
            var dialog = new Views.AddGradeWindowxaml();
            // check if a student is selected before opening the dialog
            if (dialog.ShowDialog() == true && SelectedStudent != null)
            {
                // if dialog is confirmed, add a new grade
                AddGrade(dialog.Subject, dialog.Score, SelectedStudent.StudentId);

            }
            else
            {
                // logging the cancellation of the dialog
                Debug.WriteLine("Add Grade dialog was cancelled.");
            }

        }

        private void AddGrade(string subject, double score, int studentId)
        {
            // check if a student is selected
            if (SelectedStudent != null)
            {
                // create a new grade for the selected student
                var newGrade = new Grade { Subject = subject, Score = score, StudentId = studentId };
                // add the new grade to the student's grades collection
                SelectedStudent.Grades.Add(newGrade);
                // add the new grade to the database
                _dbContext.Grades.Add(newGrade);
                try
                {
                    _dbContext.SaveChanges(); // save changes to the database
                    Debug.WriteLine($"New grade added for student: {SelectedStudent.Name}");
                    // add the new grade to the Grades collection for UI binding
                    Grades.Add(newGrade);
                    // update the selected student's total score and average score
                    SelectedStudent.RaiseScoreChanged();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving new grade: {ex.Message}");
                }
                
            }
        }

        private void SaveStudent()
        {
            // check if a student is selected
            if (SelectedStudent != null)
            {
                // logging the saving of the selected student
                Debug.WriteLine($"Saving student: {SelectedStudent.Name}");
                //find the existing student in the database
                var existingStudent = _dbContext.Students.Find(SelectedStudent.StudentId);
                if (existingStudent != null)
                {
                    // update the existing student's properties
                    existingStudent.Name = SelectedStudent.Name;
                    // update the grades collection
                    existingStudent.Grades = SelectedStudent.Grades;
                }
                else
                {
                    // if not found, add the selected student as a new entry
                    _dbContext.Students.Add(SelectedStudent);
                }

                try
                {
                    _dbContext.SaveChanges(); // save changes to the database
                    Debug.WriteLine($"Student saved: {SelectedStudent.Name}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error saving student: {ex.Message}");
                }
            }
        }

        private void RemoveStudent()
        {
            // check if a student is selected
            if (SelectedStudent != null)
            {
                // remove the selected student from the database
                _dbContext.Students.Remove(SelectedStudent);
                try
                {
                    _dbContext.SaveChanges(); // save changes to the database
                    Debug.WriteLine($"Student removed: {SelectedStudent.Name}");
                    // remove the selected student from the collection
                    Students.Remove(SelectedStudent);
                    SelectedStudent = null; // clear selection after removal
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error removing student: {ex.Message}");
                }
            }
        }

        private void AddStudent()
        {
            // logging the addition of a new student
            Debug.WriteLine("Adding a new student to the collection.");
            // add a new student to the collection
            var newStudent = new Student { 
                Name = $"新學生",
                ClassName = "新班級"
            };
            Students.Add(newStudent);

            // add the new student to the database
            _dbContext.Students.Add(newStudent);
            try
            {
                _dbContext.SaveChanges(); // save changes to the database
                Debug.WriteLine($"New student added: {newStudent.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving new student: {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
