using UartClientApp.Models;

namespace UartClientApp.Services;

/// <summary>
/// 定义串口通讯接口
/// </summary>
public interface IUartService
{
    /// <summary>
    /// 串口是否已打开
    /// </summary>
    bool IsOpen { get; }

    /// <summary>
    /// 当前的 UART 设置
    /// </summary>
    UartSettings? CurrentSettings { get; }

    /// <summary>
    /// 打开串口连接
    /// </summary>
    /// <param name="settings">UART 设置参数</param>
    /// <exception cref="IOException">打开串口失败时抛出</exception>
    void Open(UartSettings settings);

    /// <summary>
    /// 关闭串口连接
    /// </summary>
    void Close();

    /// <summary>
    /// 向串口发送数据
    /// </summary>
    /// <param name="data">要发送的数据</param>
    /// <exception cref="IOException">发送失败时抛出</exception>
    void Send(string data);

    /// <summary>
    /// 收到数据时的事件
    /// </summary>
    event EventHandler<DataReceivedEventArgs>? DataReceived;

    /// <summary>
    /// 连接错误时的事件
    /// </summary>
    event EventHandler<ErrorOccurredEventArgs>? ErrorOccurred;
}

/// <summary>
/// 数据接收事件参数
/// </summary>
public class DataReceivedEventArgs : EventArgs
{
    public string Data { get; set; } = string.Empty;
}

/// <summary>
/// 错误发生事件参数
/// </summary>
public class ErrorOccurredEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
}
