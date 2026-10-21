using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GP_Auto_Accounts
{
    public static class AppContext
    {
        public static UserInfo CurrentUser { get; set; }
    }

    public class UserInfo
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string UserType { get; set; }
    }
}
