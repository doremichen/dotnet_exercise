using DocumentFormat.OpenXml.Spreadsheet;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TimetableGenerator.Models;

namespace TimetableGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.DataContext is TimetableCell cell)
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "請選擇科目 JSON 檔案",
                    Filter = "JSON Files (*.json)|*.json",
                    DefaultExt = ".json"
                };

                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        var json = File.ReadAllText(dialog.FileName);
                        var subjects = JsonSerializer.Deserialize<List<Subject>>(json);

                        if (subjects?.Count > 0)
                        {
                            var subjectNames = subjects.Select(s => s.Name).ToList();
                            var selected = Microsoft.VisualBasic.Interaction.InputBox(
                                "請輸入課程名稱：\n" + string.Join("\n", subjectNames),
                                "選擇課程", subjectNames[0]);

                            var chosen = subjects.FirstOrDefault(s => s.Name == selected);
                            if (chosen != null)
                            {
                                cell.Subject = chosen;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"讀取 JSON 失敗：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                // 關閉下拉選單（避免顯示空白）
                comboBox.IsDropDownOpen = false;
            }
        }
    }
}