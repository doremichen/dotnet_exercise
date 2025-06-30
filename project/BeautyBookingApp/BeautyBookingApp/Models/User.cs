/**
 * Description: This file defines the User class for the BeautyBookingApp project.
 * Author: Adam Chen
 * Date: 2025-06-30
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeautyBookingApp.Models
{
    public class User
    {
        // username and password for authentication
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;

        // future properties: FullName, Email, PhoneNumber, etc.
        public string FullName { get; set; } = String.Empty;
        public string Email { get; set; } = String.Empty;
        public string PhoneNumber { get; set; } = String.Empty;
    }
}
