using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.IO;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;
using UartClientApp.Models;
using UartClientApp.Services;

namespace UartClientApp.ViewModels;

/// <summary>
/// 主窗口视图模型
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IUartService _uartService;
    private readonly IPortService _portService;
    private readonly IChecksumService _checksumService;
    private readonly DispatcherTimer _connectionMonitorTimer;
    private readonly List<string> _logList = new();

    // UART 参数选项
    [ObservableProperty] private int[] baudRateOptions = new[] { 9600, 19200, 38400, 57600, 115200 };
    [ObservableProperty] private int[] dataBitsOptions = new[] { 5, 6, 7, 8 };
    [ObservableProperty] private Parity[] parityOptions = new[] { Parity.None, Parity.Odd, Parity.Even, Parity.Mark, Parity.Space };
    [ObservableProperty] private StopBits[] stopBitsOptions = new[] { StopBits.One, StopBits.Two };
    [ObservableProperty] private string[] checksumTypeOptions = Array.Empty<string>();

    // 选定的 UART 参数
    [ObservableProperty] private string? selectedPortName;
    [ObservableProperty] private int selectedBaudRate = 115200;
    [ObservableProperty] private int selectedDataBits = 8;
    [ObservableProperty] private Parity selectedParity = Parity.None;
    [ObservableProperty] private StopBits selectedStopBits = StopBits.One;
    [ObservableProperty] private string selectedChecksumType = "Sum (8-Bit Hex, %256)";

    // 显示相关属性
    [ObservableProperty] private string connectionStatus = "状态: 未连接";
    [ObservableProperty] private string connectionStatusColor = "Red";
    [ObservableProperty] private string connectButtonText = "开启连线 (Connect)";
    [ObservableProperty] private string receivedData = string.Empty;
    [ObservableProperty] private string sendDataText = string.Empty;
    [ObservableProperty] private string logText = string.Empty;
    [ObservableProperty] private bool isConnected = false;
    [ObservableProperty] private bool isSendEnabled = false;
    [ObservableProperty] private bool areControlsEnabled = true;

    // 端口列表
    [ObservableProperty] private string[] availablePorts = Array.Empty<string>();

    public MainWindowViewModel()
    {
        _uartService = new UartService();
        _portService = new PortService();
        _checksumService = new ChecksumService();

        // 设置 Checksum 类型选项
        ChecksumTypeOptions = _checksumService.GetSupportedChecksumTypes();

        // 设置连接监测 Timer
        _connectionMonitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _connectionMonitorTimer.Tick += ConnectionMonitorTimer_Tick;

        // 订阅 UART 服务事件
        _uartService.DataReceived += UartService_DataReceived;
        _uartService.ErrorOccurred += UartService_ErrorOccurred;

        // 初始化可用端口
        RefreshAvailablePorts();

        AppendLog("SYSTEM", "应用程序已启动。");
    }

    [RelayCommand]
    public void RefreshPorts()
    {
        RefreshAvailablePorts();
    }

    [RelayCommand]
    public void Connect()
    {
        if (!IsConnected)
        {
            if (string.IsNullOrWhiteSpace(SelectedPortName))
            {
                MessageBox.Show("请选择序列埠名称 (Port Name)!", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var settings = new UartSettings(
                    SelectedPortName,
                    SelectedBaudRate,
                    SelectedDataBits,
                    SelectedParity,
                    SelectedStopBits
                );

                _uartService.Open(settings);
                UpdateConnectionState(true);
                _connectionMonitorTimer.Start();

                var currentSettings = _uartService.CurrentSettings;
                if (currentSettings != null)
                {
                    AppendLog("CONNECT",
                        $"成功连线至 {currentSettings.PortName} " +
                        $"({currentSettings.BaudRate}, " +
                        $"{currentSettings.DataBits}, " +
                        $"{currentSettings.Parity}, " +
                        $"{currentSettings.StopBits})");
                }
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"开启连线失败: {ex.Message}");
                MessageBox.Show($"无法开启序列埠: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            Disconnect("使用者手动断开连线");
        }
    }

    [RelayCommand]
    public void Send()
    {
        if (!IsConnected)
        {
            HandleUnexpectedDisconnect("连线已中断，无法传送。");
            return;
        }

        if (!string.IsNullOrWhiteSpace(SendDataText))
        {
            try
            {
                string rawText = SendDataText;
                string checksumStr = _checksumService.CalculateChecksum(rawText, SelectedChecksumType);

                string sendData;
                string logMessage;

                if (!string.IsNullOrEmpty(checksumStr))
                {
                    sendData = $"{rawText}*{checksumStr}\r\n";
                    logMessage = $"{rawText}*[CS:{checksumStr}] (\\r\\n)";
                }
                else
                {
                    sendData = $"{rawText}\r\n";
                    logMessage = $"{rawText} (\\r\\n)";
                }

                _uartService.Send(sendData);
                AppendLog("TX", logMessage);
                SendDataText = string.Empty;
            }
            catch (IOException ioEx)
            {
                HandleUnexpectedDisconnect(ioEx.Message);
            }
            catch (UnauthorizedAccessException authEx)
            {
                HandleUnexpectedDisconnect(authEx.Message);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"传送失败: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public void ClearReceived()
    {
        ReceivedData = string.Empty;
        AppendLog("SYSTEM", "已清除接收资料区。");
    }

    [RelayCommand]
    public void ClearLog()
    {
        LogText = string.Empty;
        _logList.Clear();
        AppendLog("SYSTEM", "已清除 Log View。");
    }

    [RelayCommand]
    public void ExportLog()
    {
        if (string.IsNullOrWhiteSpace(LogText))
        {
            MessageBox.Show("目前没有可供列出的 Log 纪录!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        Microsoft.Win32.SaveFileDialog saveFileDialog = new()
        {
            Title = "列出系统与通讯 Log",
            Filter = "文字档案 (*.txt)|*.txt|所有档案 (*.*)|*.*",
            DefaultExt = "txt",
            FileName = $"UartLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                File.WriteAllText(saveFileDialog.FileName, LogText, System.Text.Encoding.UTF8);
                AppendLog("SYSTEM", $"Log 档案已成功列出至: {saveFileDialog.FileName}");
                MessageBox.Show($"Log 列出成功!\n储存路径: {saveFileDialog.FileName}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendLog("ERROR", $"列出 Log 失败: {ex.Message}");
                MessageBox.Show($"储存档案时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    // 专用方法（不需要通过命令调用）
    private void Disconnect(string reason)
    {
        _connectionMonitorTimer.Stop();

        try
        {
            _uartService.Close();
            UpdateConnectionState(false);
            AppendLog("DISCONNECT", $"已断开连线 - 原因: {reason}");
        }
        catch (Exception ex)
        {
            AppendLog("ERROR", $"关闭连线出错: {ex.Message}");
        }
    }

    private void HandleUnexpectedDisconnect(string errorMessage)
    {
        if (!IsConnected)
        {
            return;
        }

        _connectionMonitorTimer.Stop();

        try
        {
            _uartService.Close();
        }
        catch { }

        UpdateConnectionState(false);
        RefreshAvailablePorts();

        AppendLog("ERROR", $"通讯中断: {errorMessage}");
        MessageBox.Show($"装置已离线或连线中断!\n详细资讯: {errorMessage}", "通讯中断", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    private void UpdateConnectionState(bool isConnected)
    {
        IsConnected = isConnected;
        IsSendEnabled = isConnected;
        AreControlsEnabled = !isConnected;
        ConnectButtonText = isConnected ? "断开连线 (Disconnect)" : "开启连线 (Connect)";
        ConnectionStatus = isConnected ? "状态: 已连线" : "状态: 未连线";
        ConnectionStatusColor = isConnected ? "Green" : "Red";
    }

    private void RefreshAvailablePorts()
    {
        AvailablePorts = _portService.GetAvailablePorts();
        if (AvailablePorts.Length > 0 && string.IsNullOrEmpty(SelectedPortName))
        {
            SelectedPortName = AvailablePorts[0];
        }

        AppendLog("SYSTEM", $"重新整理 Port 清单，发现 {AvailablePorts.Length} 个序列埠。");
    }

    private void ConnectionMonitorTimer_Tick(object? sender, EventArgs e)
    {
        if (!_uartService.IsOpen)
        {
            HandleUnexpectedDisconnect("检测到序列埠已关闭。");
            return;
        }

        if (!_portService.PortExists(_uartService.CurrentSettings?.PortName ?? string.Empty))
        {
            HandleUnexpectedDisconnect($"装置 {_uartService.CurrentSettings?.PortName} 已从系统移除。");
        }
    }

    private void UartService_DataReceived(object? sender, DataReceivedEventArgs e)
    {
        ReceivedData += e.Data;
        AppendLog("RX", e.Data);
    }

    private void UartService_ErrorOccurred(object? sender, ErrorOccurredEventArgs e)
    {
        HandleUnexpectedDisconnect(e.Message);
    }

    private void AppendLog(string category, string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string line = $"[{timestamp}] [{category}] {message}{Environment.NewLine}";

        _logList.Add(line);
        LogText = string.Concat(_logList);
    }
}
