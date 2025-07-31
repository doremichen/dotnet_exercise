/**
 * 
 * Description: This class is the TimetableCell model, this model contains the day, period, and subject assigned to that cell.
 * 
 * Author: Adam Chen
 * Date: 2025/07/31
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimetableGenerator.Models
{
    /**
     * enum TimeSection: Mornig and Afternoon
     */
    public enum TimeSection
    {
        Morning = 0,
        Afternoon = 1
    }


    public class TimetableCell : INotifyPropertyChanged
    {
        // Day of the week (e.g., Monday, Tuesday)
        public int Day { get; set; }

        // Period of the day (e.g., 1, 2, 3)
        public int Period { get; set; }

        // Time section of the day (Morning or Afternoon)
        public TimeSection Section { get; set; } = TimeSection.Morning;

        // Subject assigned to this cell
        private Subject? _subject;
        public Subject? Subject
        {
            get => _subject;
            set
            {
                if (_subject != value)
                {
                    _subject = value;
                    OnPropertyChanged(nameof(Subject));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
