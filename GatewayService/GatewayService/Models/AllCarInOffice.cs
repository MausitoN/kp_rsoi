using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Models
{
    public class AllCarInOffice
    {
        public Guid OfficeUid { get; set; }
        public string LocationOffice { get; set; }
        public Guid CarUid { get; set; }
        public long RegistrationNumber { get; set; }
        public DateTime AvailabilityScheduleFirst { get; set; }
        public DateTime AvailabilityScheduleSecond { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Power { get; set; }
        public CarType CarType { get; set; }
    }
}
