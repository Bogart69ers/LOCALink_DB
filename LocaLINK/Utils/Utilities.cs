using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LocaLINK.Repository;


namespace LocaLINK.Utils
{
    public enum ErrorCode
    {
        Success,
        Error
    }
    public enum status
    {
        InActive,
        Active
    }

    public enum RoleType
    {
        User,
        Worker,
        Admin
    }

    public enum ProductStatus
    {
        NoStock,
        HasStock
    }

    public enum OrderStatus
    {
        Open,
        Pending,
        Paid,
        Delivered,
        Close
    }

    public class Constant
    {
        public const string Role_User = "User";
        public const string Role_Worker = "Worker";
        public const string Role_Admin = "Admin";

        public const int ERROR = 1;
        public const int SUCCESS = 0;

        public const string X = "X";
        public const string MINUS = "−";
        public const string PLUS = "+";
    }

    public class Utilities
    {
        public static String gUid
        {
            get
            {
                return Guid.NewGuid().ToString();
            }
        }
        // Return random number for OTP
        public static int code
        {
            get
            {
                Random r = new Random();
                return r.Next(100000, 999999);
            }
        }

        public static List<SelectListItem> ListRole
        {
            get
            {
                BaseRepository<User_Role> role = new BaseRepository<User_Role>();
                var list = new List<SelectListItem>();
                foreach (var item in role.GetAll())
                {
                    var r = new SelectListItem
                    {
                        Text = item.rolename,
                        Value = item.roleId.ToString()
                    };

                    list.Add(r);
                }

                return list;
            }
        }

       
    }
}