/**
 * Description: This class is Utils for the BeautyBookingApp project.
 * 
 * Author: Adam Chen
 * Date: 2025/08/04
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Utils
{
    public static class AppDataHelper
    {
        /// <summary>
        /// 取得位於 %LocalAppData%\<AppName>\<fileName> 的完整路徑。
        /// 自動建立目錄（如不存在）。
        /// </summary>
        public static string GetAppDataFilePath(string fileName)
        {
            string appName = Assembly.GetExecutingAssembly().GetName().Name ?? "AdamApp";
            string folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                appName);

            Directory.CreateDirectory(folderPath); // 確保資料夾存在

            return Path.Combine(folderPath, fileName);
        }
    }
}
