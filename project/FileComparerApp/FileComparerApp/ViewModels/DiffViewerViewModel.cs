/** FileComparerApp - A simple file comparison application.
 * 
 * Description: This class is DiffViewerViewModel, which is the ViewModel for the diff viewer in the FileComparerApp.
 * 
 * Author: Adam Chen
 * Date: 2025/07/28
 */
using FileComparerApp.Models;
using FileComparerApp.Services;
using FileComparerApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileComparerApp.ViewModels
{
    public class DiffViewerViewModel : INotifyPropertyChanged
    {
        // Observable collections for left and right diff lines
        private ObservableCollection<DiffLine> _leftDiffLines = new ObservableCollection<DiffLine>();
        private ObservableCollection<DiffLine> _rightDiffLines = new ObservableCollection<DiffLine>();

        public ObservableCollection<DiffLine> LeftDiffLines
        {
            get => _leftDiffLines;
            set
            {
                if (_leftDiffLines != value)
                {
                    _leftDiffLines = value;
                    OnPropertyChanged(nameof(LeftDiffLines));
                }
            }
        }

        public ObservableCollection<DiffLine> RightDiffLines
        {
            get => _rightDiffLines;
            set
            {
                if (_rightDiffLines != value)
                {
                    _rightDiffLines = value;
                    OnPropertyChanged(nameof(RightDiffLines));
                }
            }
        }

        /**
         * Load method to load the diff lines from the specified files.
         */
        public void Load(string leftFile, string rightFile)
        {
            Util.Log($"Loading diff for files: {leftFile} and {rightFile}");
            var left = File.Exists(leftFile) ? File.ReadAllLines(leftFile) : Array.Empty<string>();
            var right = File.Exists(rightFile) ? File.ReadAllLines(rightFile) : Array.Empty<string>();

            IContentDiffer differ = new LineByLineDiffer();
            var (leftLines, rightLines) = differ.Compare(left, right);

            Util.Log($"Loaded {leftLines.Count()} left lines and {rightLines.Count()} right lines for comparison.");

            LeftDiffLines.Clear();
            RightDiffLines.Clear();

            Util.Log($"Cleared existing diff lines. Adding new lines to collections.");

            foreach (var l in leftLines) LeftDiffLines.Add(l);
            foreach (var r in rightLines) RightDiffLines.Add(r);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
