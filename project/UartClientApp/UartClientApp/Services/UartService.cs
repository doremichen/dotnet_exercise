using System.IO.Ports;
using UartClientApp.Models;

namespace UartClientApp.Services;

/// <summary>
/// 串口通讯服务的具体实现
/// </summary>
public class UartService : IUartService
{
    private readonly SerialPort _serialPort = new SerialPort();
    private readonly object _syncLock = new object();

    public bool IsOpen
    {
        get
        {
            lock (_syncLock)
            {
                return _serialPort.IsOpen;
            }
        }
    }

    public UartSettings? CurrentSettings { get; private set; }

    public event EventHandler<DataReceivedEventArgs>? DataReceived;
    public event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;

    public UartService()
    {
        _serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Open(UartSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        lock (_syncLock)
        {
            if (_serialPort.IsOpen)
            {
                Close();
            }

            try
            {
                _serialPort.PortName = settings.PortName;
                _serialPort.BaudRate = settings.BaudRate;
                _serialPort.DataBits = settings.DataBits;
                _serialPort.Parity = settings.Parity;
                _serialPort.StopBits = settings.StopBits;

                _serialPort.Open();
                CurrentSettings = settings;
            }
            catch (Exception)
            {
                CurrentSettings = null;
                throw;
            }
        }
    }

    public void Close()
    {
        lock (_syncLock)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
            }
            finally
            {
                CurrentSettings = null;
            }
        }
    }

    public void Send(string data)
    {
        ArgumentNullException.ThrowIfNull(data);

        lock (_syncLock)
        {
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开，无法发送数据");
            }

            try
            {
                _serialPort.Write(data);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs 
                { 
                    Message = $"发送数据失败: {ex.Message}",
                    Exception = ex
                });
                throw;
            }
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            lock (_syncLock)
            {
                if (_serialPort.IsOpen)
                {
                    string data = _serialPort.ReadExisting();
                    DataReceived?.Invoke(this, new DataReceivedEventArgs { Data = data });
                }
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, new ErrorOccurredEventArgs
            {
                Message = $"接收数据出错: {ex.Message}",
                Exception = ex
            });
        }
    }
}
