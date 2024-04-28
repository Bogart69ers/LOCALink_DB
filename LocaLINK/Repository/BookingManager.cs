using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocaLINK.Utils;

namespace LocaLINK.Repository
{
    public class BookingManager
    {
        LOCALinkEntities1 _db;
        BaseRepository<Booking> _book;
        UserManager _userMgr;


        public BookingManager()
        {
            _db = new LOCALinkEntities1();
            _book = new BaseRepository<Booking>();
            _userMgr = new UserManager();
        }
        
        public List<Booking> GetBookingByUserId(String customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id && m.status == (Int32)BookingStatus.Pending).ToList();
        }

        public ErrorCode CreateBookingService(Booking bookingnm, string username, ref string errMsg)
        {
            var userinf = _userMgr.GetUserInfoByUsername(username);

            bookingnm.customer_id = userinf.userId;
            bookingnm.status = (int)BookingStatus.Pending;

            if (_book.Create(bookingnm, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;

        }

        internal ErrorCode CreateBookingByUserId(Booking booking, ref string errorMessage)
        {
            throw new NotImplementedException();
        }
        public Booking GetUserCustomerByUserId(String customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id).FirstOrDefault();
        }
        public Booking CreateOrRetrieveBooking(String username, ref String err)
        {
            var User = _userMgr.GetUserInfoByUsername(username);
            var book = GetUserCustomerByUserId(User.userId);

            if (book != null)
                return book;

            book = new Booking();
            book.customer_id = User.userId;

            return GetUserCustomerByUserId(User.userId);
        }
    }
}