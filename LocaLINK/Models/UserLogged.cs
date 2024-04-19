using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LocaLINK.Models
{
    public class UserLogged
    {
        public User_Account UserAccount { get; set; }
        public User_Info UserInformation { get; set; }
    }
}