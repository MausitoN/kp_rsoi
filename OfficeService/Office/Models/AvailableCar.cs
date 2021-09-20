using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Models
{
    public class AvailableCar
    {
        public long Id { get; set; }
        public Guid CarUid { get; set; }
        public long RegistrationNumber { get; set; }
        public DateTime AvailabilityScheduleFirst { get; set; }
        public DateTime AvailabilityScheduleSecond { get; set; }
        public Guid OfficeId { get; set; }
        public RentOffice RentOffice { get; set; }
    }
}
