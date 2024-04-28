using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocaLINK.Utils;
using System.Web.Mvc;

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

        public static List<SelectListItem> ListsServices
        {
            get
            {
                BaseRepository<Services> service = new BaseRepository<Services>();
                var list = new List<SelectListItem>();
                foreach (var item in service.GetAll())
                {
                    var r = new SelectListItem
                    {
                        Text = item.serviceName,
                        Value = item.id.ToString()
                    };

                    list.Add(r);
                }

                return list;
            }
        }

    }
}