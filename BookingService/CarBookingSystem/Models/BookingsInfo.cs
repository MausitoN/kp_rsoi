using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public class BookingsInfo
    {
        public Guid BookingUid { get; set; }
        public string RegistrationNumber { get; set; }
        public BookingStatus Status { get; set; }
        public Guid PaymentUid { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int Price { get; set; }
    }
}
