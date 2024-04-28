using LocaLINK.Contracts;
using LocaLINK.Repository;
using LocaLINK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LocaLINK.Controllers
{
    [Authorize(Roles = "User, Worker")]
    public class HomeController : BaseController
    {
        [AllowAnonymous]
        // GET: Home
        public ActionResult Index()
        {
            IsUserLoggedSession();

            return View();
        }

        [AllowAnonymous]
        public ActionResult Login(String ReturnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }

            ViewBag.Error = String.Empty;
            ViewBag.ReturnUrl = ReturnUrl;

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(String username, String password, String ReturnUrl)
        {
            if (_userManager.Login(username, password, ref ErrorMessage) == ErrorCode.Success)
            {
                var user = _userManager.GetUserByUsername(username);

                if (user.status != (Int32)status.Active)
                {
                    TempData["username"] = username;
                    return RedirectToAction("Verify");
                }
                // 
                FormsAuthentication.SetAuthCookie(username, false);
                //
                if (!String.IsNullOrEmpty(ReturnUrl))
                    return Redirect(ReturnUrl);

                switch (user.User_Role.rolename)
                {
                    case Constant.Role_User:

                        return RedirectToAction("Booking");
                    case Constant.Role_Worker:

                        return RedirectToAction("Worker");
                    case Constant.Role_Admin:

                        return RedirectToAction("Index");
                    default:
                        return RedirectToAction("Index");
                }
            }
            ViewBag.Error = ErrorMessage;

            return View();
        }
        [AllowAnonymous]
        public ActionResult Verify()
        {
            if (String.IsNullOrEmpty(TempData["username"] as String))
                return RedirectToAction("Login");

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Verify(String code, string username)
        {
            if (String.IsNullOrEmpty(username))
                return RedirectToAction("Login");

            TempData["username"] = username;

            var user = _userManager.GetUserByUsername(username);

            if (!user.code.Equals(code))
            {
                TempData["error"] = "Incorrect Code";
                return View();
            }

            user.status = (Int32)status.Active;
            _userManager.UpdateUser(user, ref ErrorMessage);

            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public ActionResult SignUp()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            ViewBag.Role = Utilities.ListRole;

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SignUp(User_Account ua, string ConfirmPass)
        {
            if (!ua.password.Equals(ConfirmPass))
            {
                ModelState.AddModelError(String.Empty, "Password not match");
                ViewBag.Role = Utilities.ListRole;
                return View(ua);
            }

            if (_userManager.SignUp(ua, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);

                ViewBag.Role = Utilities.ListRole;
                return View(ua);
            }

            var user = _userManager.GetUserByEmail(ua.email);
            string verificationCode = ua.code;

            string emailBody = $"Your verification code is: {verificationCode}";
            string errorMessage = "";

            var mailManager = new MailManager();
            bool emailSent = mailManager.SendEmail(ua.email, "Verification Code", emailBody, ref errorMessage);

            if (!emailSent)
            {
                ModelState.AddModelError(String.Empty, errorMessage);
                ViewBag.Role = Utilities.ListRole;
                return View(ua);
            }
            TempData["username"] = ua.username;
            return RedirectToAction("Verify");
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [AllowAnonymous]
        public ActionResult MyProfile()
        {
            IsUserLoggedSession();

            var username = User.Identity.Name;
            var user = _userManager.CreateOrRetrieve(User.Identity.Name, ref ErrorMessage);
            var userEmail = _userManager.GetUserByEmail(user.email);

            ViewBag.userEmail = userEmail.email;
            return View(user);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult MyProfile(User_Info userInf)
        {
            var userEmail = _userManager.GetUserByEmail(userInf.email);

            ViewBag.userEmail = userEmail.email;

            if (_userManager.UpdateUserInformation(userInf, ref ErrorMessage) == ErrorCode.Error)
            {
                //
                ModelState.AddModelError(String.Empty, ErrorMessage);
                //
                return View(userInf);
            }
            TempData["Message"] = $"User Information {ErrorMessage}!";
            return View(userInf);

        }
        [AllowAnonymous]
        public ActionResult Booking()
        {
<<<<<<< Updated upstream
            return View();
=======
            IsUserLoggedSession();
            var username = User.Identity.Name;
            var user = _bookingMng.CreateOrRetrieveBooking(User.Identity.Name, ref ErrorMessage);
            var id = _userManager.GetUserInfoByUsername(username);

            ViewBag.booking = id.userId;
            ViewBag.Service = ServicesManager.ListsServices;
            return View(user);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Booking(Booking _book)
        {
            var username = User.Identity.Name;
            var user = _userManager.GetUserInfoByUserId(UserId);

            if (_bookingMng.CreateBookingService(_book, username, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);

                ViewBag.Service = ServicesManager.ListsServices;
                return View(_book);
            }
            ViewBag.Service = ServicesManager.ListsServices;
            return View(_book);
>>>>>>> Stashed changes
        }
        [AllowAnonymous]
        public ActionResult Worker()
        {
            return View();
        }
    }
}