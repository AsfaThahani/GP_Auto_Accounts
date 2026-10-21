using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP_Auto_Accounts
{
    // Static class to hold global application state
    public static class AppContext
    {
        public static UserInfo CurrentUser { get; set; }
    }

    // User information class
    public class UserInfo
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
    }
}
