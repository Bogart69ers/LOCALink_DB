using LocaLINK.Contracts;
using LocaLINK.Repository;
using LocaLINK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
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
                        return RedirectToAction("Dashboard");
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
        public ActionResult Progress()
        {
            IsUserLoggedSession();
            var user = _userManager.GetUserByUserId(UserId);
            ViewBag.currentuser = user.userId;// Fetch bookings for the current user
            List<Booking> bookinglist = _bookMng.GetUserBookingByUserId(user.userId);
            return View(bookinglist);
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
                // Retrieve booking by booking_id
                var bookingManager = new BookingManager();
                var booking = bookingManager.GetBookingById(id.Value);

                if (booking != null)
                {
                    // Update booking status to Confirmed
                    booking.status = (int)BookStatus.Confirmed;

                    // Update worker_id with current user_id
                    string currentUserId = _userManager.GetUserByUsername(Username).userId;
                    booking.worker_id = currentUserId;

                    // Save changes
                    string errorMessage = null;
                    var updateStatusResult = bookingManager.UpdateBookingStatus(booking, ref errorMessage);

                    if (updateStatusResult == ErrorCode.Success)
                    {
                        // Status updated successfully
                        ViewBag.SuccessMessage = "Booking status updated successfully.";
                    }
                    else
                    {
                        // Handle error
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
                ViewBag.ErrorMessage = "Invalid booking id.";
            }

            // Redirect to the Worker action to reload the map and pins
            return RedirectToAction("Worker");
        }

        [AllowAnonymous]
        public ActionResult Dashboard()
        {
            var bookingManager = new BookingManager();

            var allBookings = bookingManager.GetAllBookings();

            return View(allBookings);
        }

        [AllowAnonymous]
        public ActionResult UserAccounts()
        {
            var user = new UserManager();

            var alluser = user.GetAllBUserInfo();

            return View(alluser);
        }

        [AllowAnonymous]
        public ActionResult Edit(int id)
        {
            IsUserLoggedSession();

            var user = _userManager.RetrieveData(id, ref ErrorMessage);

            ViewBag.Role = Utilities.ListRole;
            return View(user);
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Edit(User_Account ua, int id)
        {
            ViewBag.Role = Utilities.ListRole;

            var user = _userManager.GetUserById(id);

            if (_userManager.UpdateUser(ua, ref ErrorMessage) == ErrorCode.Error)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
                return View(ua);
            }
            TempData["Message"] = $"User Account {ErrorMessage}!";
            return View(ua);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            string errorMSg;

            var del = _userAcc.Delete(id, out errorMSg);
            if (del == ErrorCode.Success)
            {
                TempData["SuccessMessage"] = $"Account successfully deleted!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete account: " + errorMSg;
            }

            return RedirectToAction("UserAccounts", new { deletedSuccessfully = del == ErrorCode.Success });
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult cancelBooking(int booking_id)
        {
            var book = _bookMng.GetbookId(booking_id);
            if (book != null)
            {
                book.status = 3;
                _bookMng.UpdateBookingStatus(book, ref ErrorMessage);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [AllowAnonymous]
        public ActionResult ProgressWorker()
        {
            IsUserLoggedSession();
            var book = _userManager.GetUserByUserId(UserId);
            ViewBag.Worker = book.userId;
            List<Booking> booklist = _bookMng.GetUserBookingByUserId(book.userId);
            return View(booklist);
        }

        [AllowAnonymous]
        public ActionResult Reports()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult SignUpAdmin()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index");

            ViewBag.Role = Utilities.ListRole;

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult SignUpAdmin(User_Account ua, string ConfirmPass)
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
            return RedirectToAction("SignUpAdmin");
        }
        [AllowAnonymous]
        public ActionResult ForgotPass(string email, string code, string newPassword, User_Account ua)
        {
            // Check if the cdbutton was clicked

            if (!string.IsNullOrEmpty(Request.Form["cdbutton"]))
            {
                // Extract email from the model
                string uemail = ua.email;

                if (string.IsNullOrEmpty(uemail))
                {
                    // Handle case where email is not provided
                    ViewBag.Error = "Email is required.";
                    return View();
                }

                var user = _userManager.GetUserByEmail(uemail);

                if (user == null)
                {
                    // Handle case where user with the provided email is not found
                    ViewBag.Error = "User with the provided email does not exist.";
                    return View();
                }

                // Retrieve the fixed verification code from the user's record in the database
                string verificationCode = user.code;

                // Send email with the fixed verification code
                string emailBody = $"Your verification code is: {verificationCode}";
                string errorMessage = "";
                var mailManager = new MailManager();
                bool emailSent = mailManager.SendEmail(email, "Verification Code", emailBody, ref errorMessage);
                if (!emailSent)
                {
                    // Handle case where email sending fails
                    ViewBag.Error = errorMessage;
                    return View();
                }


                Session["VerificationCode"] = verificationCode;
                return RedirectToAction("ForgotPass");

            }


            if (!string.IsNullOrEmpty(Request.Form["confirmButton"]))
            {
                // Compare the entered code with the stored verification code
                if (code != Session["VerificationCode"]?.ToString())
                {
                    // Handle case where the entered code is incorrect
                    ViewBag.Error = "Incorrect verification code.";
                    return View();
                }

                // Logic to update the password in the database
                var user = _userManager.GetUserByEmail(email);
                if (user == null)
                {
                    ViewBag.Error = "User not found.";
                    return View();
                }

                // Assuming you have a method like UpdatePassword in your user manager
                var passwordUpdated = _userManager.UpdatePassword(user, newPassword);
                if (passwordUpdated != ErrorCode.Success)
                {
                    ViewBag.Error = "Failed to update password.";
                    return View();
                }
                // After updating the password, clear the session
                Session.Remove("VerificationCode");

                // Redirect to a success page or another appropriate action
                return RedirectToAction("PasswordUpdated");

            }


            // Return the view for other cases (e.g., initial load or form submission without clicking cdbutton)



            // Return the view for other cases (e.g., initial load or form submission without clicking cdbutton)
            return View();
        }
    }
}