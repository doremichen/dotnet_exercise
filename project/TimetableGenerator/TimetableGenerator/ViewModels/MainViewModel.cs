using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimetableGenerator.Models;

namespace TimetableGenerator.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ObservableCollection<Subject>
        private ObservableCollection<Subject> _subjects = new ObservableCollection<Subject>();
        public ObservableCollection<Subject> Subjects
        {
            get => _subjects;
            set
            {
                if (_subjects != value)
                {
                    _subjects = value;
                    OnPropertyChanged(nameof(Subjects));
                }
            }
        }

        // ObservableCollection<TimetableCell>
        private ObservableCollection<TimetableCell> _morningTimetable = new ObservableCollection<TimetableCell>();
        public ObservableCollection<TimetableCell> MorningTimetable
        {
            get => _morningTimetable;
            set
            {
                if (_morningTimetable != value)
                {
                    _morningTimetable = value;
                    OnPropertyChanged(nameof(MorningTimetable));
                }
            }
        }

        // ObservableCollection<TimetableCell>
        private ObservableCollection<TimetableCell> _afternoonTimetable = new ObservableCollection<TimetableCell>();
        public ObservableCollection<TimetableCell> AfternoonTimetable
        {
            get => _afternoonTimetable;
            set
            {
                if (_afternoonTimetable != value)
                {
                    _afternoonTimetable = value;
                    OnPropertyChanged(nameof(AfternoonTimetable));
                }
            }
        }


        // Constructor to initialize the ViewModel
        public MainViewModel()
        {
            // 初始化 Subjects 列表
            Subjects.Add(new Subject("國語"));
            Subjects.Add(new Subject("數學"));
            Subjects.Add(new Subject("英文"));
            Subjects.Add(new Subject("自然"));
            Subjects.Add(new Subject("社會"));
            Subjects.Add(new Subject("體育"));
            Subjects.Add(new Subject("美術"));
            Subjects.Add(new Subject("音樂"));
            Subjects.Add(new Subject("電腦"));

            // 初始化上午課表
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 4; period++) // 上午 4 節課 (0-3)
                {
                    MorningTimetable.Add(new TimetableCell
                    {
                        Day = day,
                        Period = period,
                        Section = TimeSection.Morning,
                        Subject = null // 預設為空，用戶選擇後會更新
                    });
                }
            }

            // 初始化下午課表
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 3; period++) // 下午 3 節課 (0-2)
                {
                    AfternoonTimetable.Add(new TimetableCell
                    {
                        Day = day,
                        Period = period,
                        Section = TimeSection.Afternoon,
                        Subject = null // 預設為空，用戶選擇後會更新
                    });
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
