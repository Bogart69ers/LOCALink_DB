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
        public ActionResult Login(string username, string password, string returnUrl)
        {
            if (_userManager.Login(username, password, ref ErrorMessage) == ErrorCode.Success)
            {
                var user = _userManager.GetUserByUsername(username);

                if (user.status != (int)status.Active)
                {
                    TempData["username"] = username;
                    return RedirectToAction("Verify");
                }

                // Set authentication cookie
                FormsAuthentication.SetAuthCookie(username, false);

                // Redirect user based on role
                switch (user.User_Role.rolename)
                {
                    case Constant.Role_User:
                        return RedirectToAction("Booking");
                    case Constant.Role_Worker:
                        return RedirectToAction("Worker");
                    case Constant.Role_Admin:
                        return RedirectToAction("Admin");
                    default:
                        return RedirectToAction("Index");
                }
            }

            // If login fails, display error message
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
            IsUserLoggedSession();
            var username = User.Identity.Name;
            var user = _bookMng.CreateOrRetrieveBooking(User.Identity.Name, ref ErrorMessage);
            var id = _userManager.GetUserInfoByUsername(username);

            ViewBag.booking = id.userId;
            ViewBag.Service = ServiceManager.ListsServices;
            return View(user);
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Booking(Booking _book)
        {
            var username = User.Identity.Name;
            var user = _userManager.GetUserInfoByUserId(UserId);

            if (_bookMng.CreateBookingService(_book, username, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);

                ViewBag.Service = ServiceManager.ListsServices;
                return View(_book);
            }
            ViewBag.Service = ServiceManager.ListsServices;
            return View(_book);

        }
        [AllowAnonymous]
        public ActionResult Worker()
        {
            var bookingManager = new BookingManager();
            
            var allBookings = bookingManager.GetAllBookings();

            return View(allBookings);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Worker(int? id)
        {
            if (id.HasValue)
            {
                TempData["customer_id"] = id;

                // Retrieve booking by customer_id
                var bookingManager = new BookingManager();
                var book = bookingManager.GetUserCustomerBookingByUserId(id.Value);

                if (book != null)
                {
                    // Update booking status to Confirmed
                    book.status = (int)BookStatus.Confirmed; // Cast to int
                    string errorMessage = null;
                    var updateStatusResult = bookingManager.UpdateBookingStatus(book, ref errorMessage);

                    if (updateStatusResult == ErrorCode.Success)
                    {
                        // Status updated successfully
                        ViewBag.SuccessMessage = "Booking status updated successfully.";
                    }
                    else
                    {
                        // Handle case where there's an error updating the booking status
                        ViewBag.ErrorMessage = "An error occurred while updating the booking status.";
                    }
                }
                else
                {
                    // Handle case where booking is not found
                    ViewBag.ErrorMessage = "Booking not found.";
                }
            }
            else
            {
                // Handle case where id parameter is null
                ViewBag.ErrorMessage = "Invalid customer id.";
            }

            // Redirect to the Worker action to reload the map and pins
            return RedirectToAction("Worker");
        }

        [AllowAnonymous]
        public ActionResult Admin()
        {
            var bookingManager = new BookingManager();

            var allBookings = bookingManager.GetAllBookings();

            return View(allBookings);
        }
        
        [AllowAnonymous]
        public ActionResult Dashboard()
        {
            var bookingManager = new BookingManager();

            var allBookings = bookingManager.GetAllBookings();

            return View(allBookings);
        }

    }
}