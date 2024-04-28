using LocaLINK.Models;
using LocaLINK.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LocaLINK.Controllers
{
    public class BaseController : Controller
    {
        public String ErrorMessage;
        public UserManager _userManager;
        public BookingManager _bookingMng;
        
        public String Username { get { return User.Identity.Name; } }
        public String UserId { get { return _userManager.GetUserByUsername(Username).userId; } }

        public BaseController()
        {
            ErrorMessage = String.Empty;
            _userManager = new UserManager();
            _bookingMng = new BookingManager();

        }
        public void IsUserLoggedSession()
        {
            UserLogged userLogged = new UserLogged();
            if (User != null)
            {
                if (User.Identity.IsAuthenticated)
                {
                    userLogged.UserAccount = _userManager.GetUserByUsername(User.Identity.Name);
                    userLogged.UserInformation = _userManager.CreateOrRetrieve(userLogged.UserAccount.username, ref ErrorMessage);
                }
            }
            Session["User"] = userLogged;
        }
    }
}