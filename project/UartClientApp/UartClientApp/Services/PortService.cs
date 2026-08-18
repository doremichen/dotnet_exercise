using System.IO.Ports;

namespace UartClientApp.Services;

/// <summary>
/// 串口端口枚举服务的具体实现
/// </summary>
public class PortService : IPortService
{
    public string[] GetAvailablePorts()
    {
        return SerialPort.GetPortNames();
    }

    public bool PortExists(string portName)
    {
        ArgumentNullException.ThrowIfNull(portName);

        var availablePorts = GetAvailablePorts();
        return Array.Exists(availablePorts, p => p.Equals(portName, StringComparison.OrdinalIgnoreCase));
    }
}
