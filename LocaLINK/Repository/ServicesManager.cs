using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocaLINK.Utils;

namespace LocaLINK.Repository
{
    public class ServicesManager
    {
        private BaseRepository<Services> _services;
        private UserManager _userMgr;

        public ServicesManager()
        {
            _services = new BaseRepository<Services>();
            _userMgr = new UserManager();
        }

        public Services GetServicesById(int? id)
        {
            return _services.Get(id);
        }

        public List<Services> ListServices(String username)
        {
            var user = _userMgr.GetUserByUsername(username);
            return _services._table.Where(m => m.serviceId == user.userId).ToList();
        }
    }
}