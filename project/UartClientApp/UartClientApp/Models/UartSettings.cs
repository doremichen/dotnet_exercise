using System.IO.Ports;

namespace UartClientApp.Models;

/// <summary>
/// UART 参数设置模型
/// </summary>
public record UartSettings(
    string PortName,
    int BaudRate,
    int DataBits,
    Parity Parity,
    StopBits StopBits)
{
    /// <summary>
    /// 创建默认的 UART 设置
    /// </summary>
    public static UartSettings Default => new(
        "COM1",
        115200,
        8,
        Parity.None,
        StopBits.One
    );
}
