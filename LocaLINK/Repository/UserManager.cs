using LocaLINK.Contracts;
using LocaLINK.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static LocaLINK.Utils.Utilities;

namespace LocaLINK.Repository
{
    public class UserManager
    {
        private BaseRepository<User_Account> _userAcc;
        private BaseRepository<User_Info> _userInf;
        public UserManager()
        {
            _userAcc = new BaseRepository<User_Account>();
            _userInf = new BaseRepository<User_Info>();
        }
        
        public User_Account GetUserById(int Id)
        {
            return _userAcc.Get(Id);
        }
        public User_Account GetUserByUserId(String userId)
        {
            return _userAcc._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public User_Account GetUserByUsername(String username)
        {
            return _userAcc._table.Where(m => m.username == username).FirstOrDefault();
        }
        public User_Account GetUserByEmail(String email)
        {
            return _userAcc._table.Where(m => m.email == email).FirstOrDefault();
        }
        public ErrorCode Login(String username, String password, ref String errMsg)
        {
            var userSignIn = GetUserByUsername(username);
            if (userSignIn == null)
            {
                errMsg = "User not exist!";
                return ErrorCode.Error;
            }

            if (!userSignIn.password.Equals(password))
            {
                errMsg = "Password is Incorrect";
                return ErrorCode.Error;
            }

            // user exist
            errMsg = "Login Successful";
            return ErrorCode.Success;
        }

        public ErrorCode SignUp(User_Account ua, ref String errMsg)
        {
            ua.userId = Utilities.gUid;
            ua.code = Utilities.code.ToString();
            ua.date_created = DateTime.Now;
            ua.status = (Int32)status.InActive;

            if (GetUserByUsername(ua.username) != null)
            {
                errMsg = "Username Already Exist";
                return ErrorCode.Error;
            }

            if (GetUserByEmail(ua.email) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }

            if (_userAcc.Create(ua, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // use the generated code for OTP "ua.code"
            // send email or sms here...........

            return ErrorCode.Success;
        }

        public ErrorCode UpdateUser(User_Account ua, ref String errMsg)
        {
            return _userAcc.Update(ua.id, ua, out errMsg);
        }
        public ErrorCode UpdateUserInformation(User_Info ua, ref String errMsg)
        {
            return _userInf.Update(ua.id, ua, out errMsg);
        }
        public User_Info GetUserInfoById(int id)
        {
            return _userInf.Get(id);
        }
        public User_Info GetUserInfoByUsername(String username)
        {
            var userAcc = GetUserByUsername(username);
            return _userInf._table.Where(m => m.userId == userAcc.userId).FirstOrDefault();
        }
        public User_Info GetUserInfoByUserId(String userId)
        {
            return _userInf._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public User_Info CreateOrRetrieve(String username, ref String err)
        {
            var User = GetUserByUsername(username);
            var UserInfo = GetUserInfoByUserId(User.userId);

            if (UserInfo != null)
                return UserInfo;

            UserInfo = new User_Info();
            UserInfo.userId = User.userId;
            UserInfo.email = User.email;
            UserInfo.active = (Int32)status.Active;

            var userEmail = User.email;
            if (userEmail != null)
            {
                UserInfo.email = userEmail; 
            }

            _userInf.Create(UserInfo, out err);

            return GetUserInfoByUserId(User.userId);

        }
        public List<User_Account> GetAllBUserInfo()
        {
            return _userAcc.GetAll().ToList();
        }
    }
}