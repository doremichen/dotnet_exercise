/*
* MIT License
* Copyright (c) 2026 AdamChen
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*/

using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;

namespace UartClientApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly SerialPort _serialPort = new SerialPort();
    private readonly DispatcherTimer _connectionMonitorTimer = new DispatcherTimer();

    public MainWindow()
    {
        InitializeComponent();

        InitUartParameters();
        InitChecksumOptions();
        RefreshPortList();

        _serialPort.DataReceived += SerialPort_DataReceived;

        // 設定連線監測 Timer (每秒檢查一次 Port 是否被意外拔除)
        _connectionMonitorTimer.Interval = TimeSpan.FromSeconds(1);
        _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;

        AppendLog("SYSTEM", "應用程式已啟動。");
    }

    // 初始化 5 個標準 UART 參數
    private void InitUartParameters()
    {
        cmbBaudRate.ItemsSource = new int[] { 9600, 19200, 38400, 57600, 115200 };
        cmbBaudRate.SelectedItem = 115200;

        cmbDataBits.ItemsSource = new int[] { 5, 6, 7, 8 };
        cmbDataBits.SelectedItem = 8;

        cmbParity.ItemsSource = Enum.GetValues(typeof(Parity));
        cmbParity.SelectedItem = Parity.None;

        cmbStopBits.ItemsSource = new System.Collections.Generic.List<StopBits>
        {
            StopBits.One,
            StopBits.Two
        };
        cmbStopBits.SelectedItem = StopBits.One;
    }

    // 初始化 Checksum 模式選項
    private void InitChecksumOptions()
    {
        cmbChecksumType.ItemsSource = new string[]
        {
            "None (無)",
            "Sum (8-Bit Hex, %256)",  // 轉成兩位十六進位 (例如: 52 -> "34")
            "Sum (8-Bit Dec, %256)",  // 轉成十進位     (例如: 52 -> "52")
            "Sum (Full Dec)",
            "Sum (16-Bit Hex)"
        };
        cmbChecksumType.SelectedIndex = 1; // 預設使用 8-Bit Hex
    }

    // 重新整理現有的 COM Port 列表
    private void RefreshPortList()
    {
        string[] ports = SerialPort.GetPortNames();
        cmbPortName.ItemsSource = ports;
        if (ports.Length > 0 && cmbPortName.SelectedIndex == -1)
        {
            cmbPortName.SelectedIndex = 0;
        }

        AppendLog("SYSTEM", $"重新整理 Port 清單，發現 {ports.Length} 個序列埠。");
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        RefreshPortList();
    }

    // 開啟/關閉 連線控制
    private void btnConnect_Click(object sender, RoutedEventArgs e)
    {
        if (!_serialPort.IsOpen)
        {
            if (cmbPortName.SelectedItem == null)
            {
                MessageBox.Show("請選擇序列埠名稱 (Port Name)！", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _serialPort.PortName = cmbPortName.SelectedItem.ToString()!;
                _serialPort.BaudRate = cmbBaudRate.SelectedItem is int baud ? baud : 115200;
                _serialPort.DataBits = cmbDataBits.SelectedItem is int db ? db : 8;
                _serialPort.Parity = cmbParity.SelectedItem is Parity p ? p : Parity.None;
                _serialPort.StopBits = cmbStopBits.SelectedItem is StopBits sb ? sb : StopBits.One;

                _serialPort.Open();

                UpdateUiForConnectedState(true);
                _connectionMonitorTimer.Start(); // 開始背景連線監測

                AppendLog("CONNECT", $"成功連線至 {_serialPort.PortName} ({_serialPort.BaudRate}, {_serialPort.DataBits}, {_serialPort.Parity}, {_serialPort.StopBits})");
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"開啟連線失敗: {ex.Message}");
                MessageBox.Show($"無法開啟序列埠: {ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            DisconnectSerialPort("使用者手動斷開連線");
        }
    }

    // 主動關閉序列埠
    private void DisconnectSerialPort(string reason)
    {
        _connectionMonitorTimer.Stop();

        try
        {
            string portName = _serialPort.PortName;
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            UpdateUiForConnectedState(false);
            AppendLog("DISCONNECT", $"已斷開連線 ({portName}) - 原因: {reason}");
        }
        catch (Exception ex)
        {
            AppendLog("ERROR", $"關閉連線出錯: {ex.Message}");
        }
    }

    // 意外斷開（如拔除 USB）時觸發的 UI 即時更新處理
    private void HandleUnexpectedDisconnect(string errorMessage)
    {
        Dispatcher.Invoke(() =>
        {
            if (!_serialPort.IsOpen && btnConnect.Content.ToString() == "開啟連線 (Connect)")
            {
                return; // 已經是離線狀態，防重複觸發
            }

            _connectionMonitorTimer.Stop();

            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            catch { /* 忽略關閉連線過程拋出的二次例外 */ }

            UpdateUiForConnectedState(false);
            RefreshPortList(); // 自動刷新序列埠列表

            AppendLog("ERROR", $"通訊中斷: {errorMessage}");
            MessageBox.Show($"裝置已離線或連線中斷！\n詳細資訊: {errorMessage}", "通訊中斷", MessageBoxButton.OK, MessageBoxImage.Warning);
        });
    }

    // 背景 Timer 輪詢（確認實體裝置是否存在於 Windows 系統中）
    private void ConnectionMonitorTimer_Tick(object? sender, EventArgs e)
    {
        if (!_serialPort.IsOpen)
        {
            HandleUnexpectedDisconnect("檢測到序列埠已關閉。");
            return;
        }

        string[] availablePorts = SerialPort.GetPortNames();
        bool portExists = Array.Exists(availablePorts, p => p.Equals(_serialPort.PortName, StringComparison.OrdinalIgnoreCase));

        if (!portExists)
        {
            HandleUnexpectedDisconnect($"裝置 {_serialPort.PortName} 已從系統移除。");
        }
    }

    // 統一切換 UI 控制項狀態
    private void UpdateUiForConnectedState(bool isConnected)
    {
        SetControlsEnabled(!isConnected);
        btnConnect.Content = isConnected ? "斷開連線 (Disconnect)" : "開啟連線 (Connect)";
        txtStatus.Text = isConnected ? "狀態: 已連線" : "狀態: 未連線";
        txtStatus.Foreground = isConnected ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        btnSend.IsEnabled = isConnected;
    }

    private void SetControlsEnabled(bool enabled)
    {
        cmbPortName.IsEnabled = enabled;
        cmbBaudRate.IsEnabled = enabled;
        cmbDataBits.IsEnabled = enabled;
        cmbParity.IsEnabled = enabled;
        cmbStopBits.IsEnabled = enabled;
        btnRefresh.IsEnabled = enabled;
    }

    // 傳送資料事件
    private void btnSend_Click(object sender, RoutedEventArgs e)
    {
        if (!_serialPort.IsOpen)
        {
            HandleUnexpectedDisconnect("連線已中斷，無法傳送。");
            return;
        }

        if (!string.IsNullOrWhiteSpace(txtSend.Text))
        {
            try
            {
                // rawText 為欲計算 Checksum 的主體（例：/01READ）
                string rawText = txtSend.Text;
                string checksumType = cmbChecksumType.SelectedItem?.ToString() ?? "None (無)";

                // 1. 計算 Checksum (例如 8-Bit Hex 產出 "34")
                string checksumStr = CalculateChecksum(rawText, checksumType);

                string sendData;
                string logMessage;

                if (!string.IsNullOrEmpty(checksumStr))
                {
                    // 2. 封包格式： rawText + "*" + CS_H + CS_L + "\r\n"
                    sendData = $"{rawText}*{checksumStr}\r\n";
                    logMessage = $"{rawText}*[CS:{checksumStr}] (\\r\\n)";
                }
                else
                {
                    // 若選擇 None (無 Checksum)
                    sendData = $"{rawText}\r\n";
                    logMessage = $"{rawText} (\\r\\n)";
                }

                // 3. 透過 SerialPort 送出
                _serialPort.Write(sendData);

                // 4. 記錄 Log
                AppendLog("TX", logMessage);

                txtSend.Clear();
            }
            catch (IOException ioEx)
            {
                // 傳送期間拔線
                HandleUnexpectedDisconnect(ioEx.Message);
            }
            catch (UnauthorizedAccessException authEx)
            {
                HandleUnexpectedDisconnect(authEx.Message);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"傳送失敗: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 根據傳入的模式名稱計算 Checksum
    /// </summary>
    private string CalculateChecksum(string input, string type)
    {
        if (type.StartsWith("None"))
        {
            return string.Empty;
        }

        int sum = 0;
        foreach (char c in input)
        {
            sum += (int)c;
        }

        return type switch
        {
            // 8-Bit Sum 轉 2 位數 HEX 字串 (如: 52 -> "34")
            "Sum (8-Bit Hex, %256)" => (sum % 256).ToString("X2"),

            // 8-Bit Sum 轉 十進位字串 (如: 52 -> "52")
            "Sum (8-Bit Dec, %256)" => (sum % 256).ToString(),

            // 完整累加不溢位 (如: 500 -> "500")
            "Sum (Full Dec)" => sum.ToString(),

            // 16-Bit Hex (如: 500 -> "01F4")
            "Sum (16-Bit Hex)" => (sum % 65536).ToString("X4"),

            _ => string.Empty
        };
    }

    // 接收資料事件 (跨執行緒與斷線安全處理)
    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string inData = _serialPort.ReadExisting();
            Dispatcher.Invoke(() =>
            {
                txtReceived.AppendText(inData);
                txtReceived.ScrollToEnd();
            });

            AppendLog("RX", inData);
        }
        catch (IOException ioEx)
        {
            // 背景接收期間拔線
            HandleUnexpectedDisconnect(ioEx.Message);
        }
        catch (UnauthorizedAccessException authEx)
        {
            HandleUnexpectedDisconnect(authEx.Message);
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() => AppendLog("ERROR", $"接收資料出錯: {ex.Message}"));
        }
    }

    // 清除接收資料區
    private void btnClearReceived_Click(object sender, RoutedEventArgs e)
    {
        txtReceived.Clear();
        AppendLog("SYSTEM", "已清除接收資料區。");
    }

    // 清除 Log View 區
    private void btnClearLog_Click(object sender, RoutedEventArgs e)
    {
        txtLog.Clear();
        AppendLog("SYSTEM", "已清除 Log View。");
    }

    // 寫入 Log View 輔助函式
    private void AppendLog(string category, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string line = $"[{timestamp}] [{category}] {message}{Environment.NewLine}";

        if (Dispatcher.CheckAccess())
        {
            txtLog.AppendText(line);
            txtLog.ScrollToEnd();
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText(line);
                txtLog.ScrollToEnd();
            });
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _connectionMonitorTimer.Stop();

        try
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        finally
        {
            _serialPort.Dispose();
        }

        base.OnClosed(e);
    }
}