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
using System.Windows.Input;
using TimetableGenerator.Models;
using TimetableGenerator.ViewModels.Commands;

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

        // ExportWeek property
        private int _exportWeek = 1; // 預設為第 1 週
        public int ExportWeek
        {
            get => _exportWeek;
            set
            {
                if (_exportWeek != value)
                {
                    _exportWeek = value;
                    OnPropertyChanged(nameof(ExportWeek));
                }
            }
        }


        public ICommand ExportExcelCommand { get; }

        // Constructor to initialize the ViewModel
        public MainViewModel()
        {

            // ==== 初始化命令 ====
            ExportExcelCommand = new RelayCommand(ExportToExcel);

            // ==== 從 JSON 檔案讀取 Subjects 列表 ====
            // function change: LoadSubjectsFromJson();

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
                        Subject = null // 預設為空
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
                        Subject = null // 預設為空
                    });
                }
            }
        }

        private void ExportToExcel()
        {
            // dialog to select save location
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"課表_{DateTime.Today:yyyy_MM_dd}_第{ExportWeek}週.xlsx"
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                // 如果使用者取消了儲存對話框，則不進行任何操作
                return;
            }


            var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("課表");

            // 設定標題行
            ws.Cell(1, 1).Value = "星期/時段";
            ws.Cell(1, 2).Value = "上午課程";
            ws.Cell(1, 3).Value = "下午課程";

            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 2).Style.Font.Bold = true;
            ws.Cell(1, 3).Style.Font.Bold = true;

            // days of the week
            string[] daysOfWeek = { "星期一", "星期二", "星期三", "星期四", "星期五" };
            for (int i = 0; i < daysOfWeek.Length; i++)
            {
                ws.Cell(1, i + 2).Value = daysOfWeek[i];
            }
            // 設定上午和下午課程的標題
            string[] section = { "第一節", "第二節", "第三節", "第四節", "", "第一節", "第二節", "第三節", };
            for (int i = 0; i < section.Length; i++)
            {
                ws.Cell(i + 2, 1).Value = section[i];
            }

            // 上午課表
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 4; period++) // 上午 4 節課 (0-3)
                {
                    var cell = MorningTimetable.FirstOrDefault(c => c.Day == day && c.Period == period);
                    if (cell != null)
                    {
                        ws.Cell(period + 2, day + 2).Value = cell.Subject?.Name ?? "無課程";
                    }
                }
            }

            // 午休列
            ws.Cell(6, 1).Value = "午休";

            // 下午課表
            for (int day = 0; day < 5; day++) // 星期一到星期五 (0-4)
            {
                for (int period = 0; period < 3; period++) // 下午 3 節課 (0-2)
                {
                    var cell = AfternoonTimetable.FirstOrDefault(c => c.Day == day && c.Period == period);
                    if (cell != null)
                    {
                        ws.Cell(period + 7, day + 2).Value = cell.Subject?.Name ?? "無課程";
                    }
                }
            }

            // 自動調整欄寬
            ws.Columns().AdjustToContents();

            try
            {
                wb.SaveAs(saveFileDialog.FileName);
                MessageBox.Show($"課表已成功匯出到桌面: {saveFileDialog.FileName}", "匯出成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"匯出課表時發生錯誤: {ex.Message}", "匯出失敗", MessageBoxButton.OK, MessageBoxImage.Error);
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
