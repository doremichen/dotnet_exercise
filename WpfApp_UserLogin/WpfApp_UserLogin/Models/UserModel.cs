using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp_UserLogin.Models
{
    public class UserModel
    {
        // user name
        public string UserName { get; set; }
        // password
        public string Password { get; set; }

        // Authentication method
        public bool Authenticate(string username, string password)
        {
            // In a real application, you would check the credentials against a database or other data source.
            // For this example, we'll just check against hardcoded values.
            return username == UserName && password == Password;
        }


    }
}
