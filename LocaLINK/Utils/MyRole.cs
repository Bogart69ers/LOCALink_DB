using LocaLINK.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace LocaLINK
{
    public class MyRole : RoleProvider
    {
        public BaseRepository<User_Role> _role = new BaseRepository<User_Role>();
        public BaseRepository<User_Account> _userAcc = new BaseRepository<User_Account>();

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            using (var db = new LOCALinkEntities3())
            {
                return db.vw_accRole.Select(m => m.rolename).ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var db = new LOCALinkEntities3())
            {
                return db.vw_accRole.Where(m => m.username == username).Select(m => m.username).ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}