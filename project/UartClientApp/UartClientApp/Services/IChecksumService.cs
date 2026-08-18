namespace UartClientApp.Services;

/// <summary>
/// 定义校验和计算接口
/// </summary>
public interface IChecksumService
{
    /// <summary>
    /// 计算输入字符串的校验和
    /// </summary>
    /// <param name="input">输入字符串</param>
    /// <param name="checksumType">校验和类型 (需要与 UI ComboBox 中的值匹配)</param>
    /// <returns>计算结果的校验和字符串，若无需校验和则返回空字符串</returns>
    string CalculateChecksum(string input, string checksumType);

    /// <summary>
    /// 获取支持的校验和类型列表
    /// </summary>
    /// <returns>校验和类型数组</returns>
    string[] GetSupportedChecksumTypes();
}
