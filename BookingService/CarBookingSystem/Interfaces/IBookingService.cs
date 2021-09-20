using BookingService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResponseBooking> CreateBooking(CreateBookingRequest request);
        Task<ServiceResponseBooking> DeleteBooking(Guid bookingUid);
        Task<ServiceResponseBooking> FinishBooking(Guid bookingUid);
        Task<ServiceResponseBooking> GetBookingInfo(Guid bookingUid);
        Task<ServiceResponseBooking> GetBookingsInfo(Guid userUid);
    }
}
