namespace UartClientApp.Models;

/// <summary>
/// 串口消息模型，表示接收或发送的数据
/// </summary>
public record SerialMessage(
    string RawData,
    string? Checksum = null,
    MessageDirection Direction = MessageDirection.Received,
    DateTime? Timestamp = null)
{
    public string DisplayText => 
        string.IsNullOrEmpty(Checksum) 
            ? RawData 
            : $"{RawData}*{Checksum}";
}

/// <summary>
/// 消息方向枚举
/// </summary>
public enum MessageDirection
{
    Sent,
    Received
}
