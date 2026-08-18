namespace UartClientApp.Services;

/// <summary>
/// 定义串口端口枚举接口
/// </summary>
public interface IPortService
{
    /// <summary>
    /// 获取系统中所有可用的串口名称列表
    /// </summary>
    /// <returns>可用的串口名称数组</returns>
    string[] GetAvailablePorts();

    /// <summary>
    /// 检查指定的串口是否存在于系统中
    /// </summary>
    /// <param name="portName">串口名称</param>
    /// <returns>串口是否存在</returns>
    bool PortExists(string portName);
}
