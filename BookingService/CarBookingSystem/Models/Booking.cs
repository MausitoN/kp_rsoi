using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public enum BookingStatus
    {
        NEW,      // новое
        FINISHED, // завершенное
        CANCELED, // отмененное
        EXPIRED   // истекшее
    }

    public class Booking
    {
        public Guid Id { get; set; }
        public Guid CarUid { get; set; }
        public Guid UserUid { get; set; }
        public string RegistrationNumber { get; set; }
        public Guid PaymentUid { get; set; }
        public string BookingPeriod { get; set; }
        public BookingStatus Status { get; set; }
        public Guid TakeFromOfficeUid { get; set; }
        public Guid ReturnToOfficeUid { get; set; }
    }
}
