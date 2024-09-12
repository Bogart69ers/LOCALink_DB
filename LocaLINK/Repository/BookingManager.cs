using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LocaLINK.Utils;

namespace LocaLINK.Repository
{
    public class BookingManager
    {
        LOCALinkEntities3 _db;
        BaseRepository<Booking> _book;
        BaseRepository<User_Info> _info;
        BaseRepository<User_Account> _userAcc;
        UserManager _userMgr;


        public BookingManager()
        {
            _db = new LOCALinkEntities3();
            _book = new BaseRepository<Booking>();
            _userMgr = new UserManager();
            _info = new BaseRepository<User_Info>();
            _userAcc = new BaseRepository<User_Account>();
        }

        public List<Booking> GetBookingByUserId(string customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id && m.status == (int)BookStatus.Pending).ToList();
        }

        public Booking GetBookingById(int id)
        {
            return _db.Booking.Find(id); // Retrieve booking by ID
        }

        public List<Booking> GetUserBookingByUserId(string customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id).ToList();
        }

        public List<Booking> GetConfirmedBookings(string customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id && m.status == (int)BookStatus.Confirmed).ToList();
        }

        public List<Booking> GetBookingsByWorkerId(string workerId)
        {
            return _book._table.Where(m => m.worker_id == workerId).ToList();
        }

        public List<Booking> GetBookingsByAccountId(string accountId)
        {
            return _book._table.Where(m => m.customer_id == accountId).ToList();
        }
       

        public ErrorCode CreateBookingService(Booking bookingnm, string username, ref string errMsg)
        {
            var userinf = _userMgr.GetUserInfoByUsername(username);

            bookingnm.customer_id = userinf.userId;
            bookingnm.status = (int)BookStatus.Pending;

            if (_book.Create(bookingnm, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }

        public ErrorCode UpdateBookingStatus(Booking st, ref string errMsg)
        {
            return _book.Update(st.booking_id, st, out errMsg);
        }

        public Booking GetbookId(int booking_id)
        {
            return _book.Get(booking_id);
        }

        public Booking GetUserCustomerBookingByUserId(int booking_id)
        {
            return _book._table.Where(m => m.booking_id == booking_id).FirstOrDefault();
        }

        internal ErrorCode CreateBookingByUserId(Booking booking, ref string errorMessage)
        {
            throw new NotImplementedException();
        }

        public Booking GetUserCustomerByUserId(string customer_id)
        {
            return _book._table.Where(m => m.customer_id == customer_id).FirstOrDefault();
        }

        public Booking CreateOrRetrieveBooking(string username, ref string err)
        {
            var User = _userMgr.GetUserInfoByUsername(username);
            var book = GetUserCustomerByUserId(User.userId);

            if (book != null)
                return book;

            book = new Booking();
            book.customer_id = User.userId;

            return GetUserCustomerByUserId(User.userId);
        }

        public List<Booking> GetAllBookings()
        {
            return _book.GetAll().ToList();
        }
    }
}