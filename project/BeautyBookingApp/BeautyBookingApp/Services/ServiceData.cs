/**
 * Description: This file defines the ServiceData class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using BeautyBookingApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Services
{
    public class ServiceData
    {
        // List of service items by GetSrviceItems method
        public static List<ServiceItem> GetServiceItems()
        {
            return new List<ServiceItem>
            {
                new ServiceItem
                {
                    Name = "臉部清潔",
                    Description = "深層清潔臉部肌膚，去除污垢和油脂",
                    Price = 800,
                    DurationMinutes = 60
                },
                new ServiceItem
                {
                    Name = "全身按摩",
                    Description = "舒緩全身肌肉，放鬆身心",
                    Price = 1500,
                    DurationMinutes = 90
                },
                new ServiceItem
                {
                    Name = "美甲護理",
                    Description = "專業美甲設計和護理",
                    Price = 600,
                    DurationMinutes = 45
                }
            };
        }

    }
}
