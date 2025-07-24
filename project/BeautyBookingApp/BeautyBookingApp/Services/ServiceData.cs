/**
 * Description: This file defines the ServiceData class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace BeautyBookingApp.Services
{
    public class ServiceData
    {
        // List of service items by GetSrviceItems method
        public static List<ServiceItem> GetServiceItems()
        {
            // json path: "Files/beauty_service_list.json"
            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "beauty_service_list.json");

            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    // If the file does not exist, show a message box and return an empty list
                    MessageBox.Show("找不到服務清單 JSON 檔案，請確認檔案存在於指定路徑。", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                    return new List<ServiceItem>();
                }
            

                string json = File.ReadAllText(jsonFilePath);
                var items = JsonSerializer.Deserialize<List<ServiceItem>>(json);

                return items ?? new List<ServiceItem>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"讀取服務清單時發生錯誤：{ex.Message}", "錯誤", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<ServiceItem>();
            }
            
        }

    }
}
