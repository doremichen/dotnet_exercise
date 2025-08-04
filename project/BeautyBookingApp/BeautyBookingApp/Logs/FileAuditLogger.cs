using BeautyBookingApp.Models;
using System;
using System.Collections.Generic;
/**
 * Description: This class is FileAuditLogger for the BeautyBookingApp project.
 * 
 * Author: Adam Chen
 * Date: 2025/08/04
 */
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Logs
{
    public class FileAuditLogger :IAuditLogger
    {
        private readonly string _logFilePath;
        public FileAuditLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }
        public void Log(string user, string action, string target, string details = null)
        {
            try
            {
                var dir = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\tUser: {user}\tAction: {action}\tTarget: {target}\tDetails: {details}";
                File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // 可選：log 到 fallback、顯示錯誤、忽略錯誤...
                Debug.WriteLine($"[AuditLog Error] {ex.Message}");
            }
        }
    }
}
