namespace UartClientApp.Models;

/// <summary>
/// 日志条目模型
/// </summary>
public record LogEntry(
    DateTime Timestamp,
    string Category,
    string Message)
{
    public override string ToString()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Category}] {Message}";
    }
}
