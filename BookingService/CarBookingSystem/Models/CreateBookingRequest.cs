using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public class CreateBookingRequest
    {
        public Guid CarUid { get; set; }
        public string RegistrationNumber { get; set; }
        public Guid TakenFromOffice { get; set; }
        public Guid ReturnToOffice { get; set; }
        public string BookingPeriod { get; set; } //DateTime
        public Guid UserUid { get; set; }
    }
}
