namespace UartClientApp.Services;

/// <summary>
/// 校验和计算服务的具体实现
/// </summary>
public class ChecksumService : IChecksumService
{
    private static readonly string[] SupportedTypes = new[]
    {
        "None (无)",
        "Sum (8-Bit Hex, %256)",
        "Sum (8-Bit Dec, %256)",
        "Sum (Full Dec)",
        "Sum (16-Bit Hex)"
    };

    public string[] GetSupportedChecksumTypes()
    {
        return SupportedTypes;
    }

    public string CalculateChecksum(string input, string checksumType)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(checksumType);

        if (checksumType.StartsWith("None", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        int sum = 0;
        foreach (char c in input)
        {
            sum += c;
        }

        return checksumType switch
        {
            // 8-Bit Sum 转 2 位数字 HEX 字符串 (如: 52 -> "34")
            "Sum (8-Bit Hex, %256)" => (sum % 256).ToString("X2"),

            // 8-Bit Sum 转十进位字符串 (如: 52 -> "52")
            "Sum (8-Bit Dec, %256)" => (sum % 256).ToString(),

            // 完整累加不溢位 (如: 500 -> "500")
            "Sum (Full Dec)" => sum.ToString(),

            // 16-Bit Hex (如: 500 -> "01F4")
            "Sum (16-Bit Hex)" => (sum % 65536).ToString("X4"),

            _ => string.Empty
        };
    }
}
