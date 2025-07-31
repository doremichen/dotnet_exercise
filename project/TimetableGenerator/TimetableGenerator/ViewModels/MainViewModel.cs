using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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
            // ==== 從 JSON 檔案讀取 Subjects 列表 ====
            LoadSubjectsFromJson();

            // 初始化上午課表 (這部分保持不變)
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 4; period++) // 上午 4 節課 (0-3)
                {
                    MorningTimetable.Add(new TimetableCell
                    {
                        Day = day,
                        Period = period,
                        Section = TimeSection.Morning,
                        Subject = null // 預設為空
                    });
                }
            }

            // 初始化下午課表 (這部分保持不變)
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 3; period++) // 下午 3 節課 (0-2)
                {
                    AfternoonTimetable.Add(new TimetableCell
                    {
                        Day = day,
                        Period = period,
                        Section = TimeSection.Afternoon,
                        Subject = null // 預設為空
                    });
                }
            }
        }

        private void LoadSubjectsFromJson()
        {
            // json path: "Files/beauty_service_list.json"
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "subjects.json");

            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    // If the file does not exist, show a message box and return an empty list
                    MessageBox.Show("找不到服務清單 JSON 檔案，請確認檔案存在於指定路徑。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                string jsonString = File.ReadAllText(jsonFilePath);
                // 反序列化 JSON 字串為 Subject 物件的列表
                // 注意：如果 Subjects 屬性直接是一個 ObservableCollection，
                // 這裡可以直接賦值。如果它是 List<Subject> 並且您想用 ToObservableCollection() 擴展方法
                // 您需要先反序列化為 List<Subject>
                List<Subject>? loadedSubjects = JsonSerializer.Deserialize<List<Subject>>(jsonString);

                // 將讀取到的科目添加到 ObservableCollection<Subjects> 中
                // 清空現有集合，然後逐一添加，這樣 Subjects 的 NotifyCollectionChanged 事件會觸發
                // 或者直接賦值給 Subjects 屬性（推薦，但要確保 setter 是 ObservableCollection）
                if (loadedSubjects != null)
                {
                    Subjects.Clear(); // 清空現有科目 (如果之前有)
                    foreach (var subject in loadedSubjects)
                    {
                        Subjects.Add(subject);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // 處理檔案找不到的錯誤
                System.Diagnostics.Debug.WriteLine($"Error: Subject JSON file not found at {jsonFilePath}");
                // 顯示錯誤訊息給使用者
                MessageBox.Show("找不到科目 JSON 檔案，請確認檔案存在於指定路徑。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (JsonException ex)
            {
                // 處理 JSON 解析錯誤
                System.Diagnostics.Debug.WriteLine($"Error parsing subjects.json: {ex.Message}");
                // 顯示錯誤訊息給使用者
                MessageBox.Show($"無法解析科目 JSON 檔案: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                // 處理其他潛在錯誤
                System.Diagnostics.Debug.WriteLine($"An unexpected error occurred: {ex.Message}");
                // 顯示錯誤訊息給使用者
                MessageBox.Show($"發生意外錯誤: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
