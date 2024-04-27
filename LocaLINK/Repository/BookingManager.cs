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

        public Booking CreateBookingByUserId(String customer_id, Service_Provider sp, ref String err)
        {
            var user = _userMgr.GetUserInfoByUserId(sp.user_id);

            var book = _book._table.Where(m => m.customer_id == customer_id).FirstOrDefault();
            if (book == null || book.status != (Int32)BookingStatus.Pending)
            {

                book = new Booking();
                book.customer_id = customer_id;
                book.status = (Int32)BookingStatus.Pending;

                _book.Create(book, out err);

                return book;
            }
            return book;
        }

        public List<Booking> GetBookingByUserId(String customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id && m.status == (Int32)BookingStatus.Pending).ToList();
        }

        internal ErrorCode CreateBookingByUserId(Booking booking, ref string errorMessage)
        {
            throw new NotImplementedException();
        }
    }
}