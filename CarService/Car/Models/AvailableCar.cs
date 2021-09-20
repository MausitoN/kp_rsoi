using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarService.Models
{
    public class AvailableCar
    {
        public Guid CarUid { get; set; }
        public long RegistrationNumber { get; set; }
        public DateTime AvailabilitySchedule { get; set; }
    }
}
