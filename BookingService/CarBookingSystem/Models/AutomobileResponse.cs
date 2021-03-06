using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public enum CarType
    {
        SEDAN,
        SUV,
        MINIVAN,
        ROADSTER
    }
    public class AutomobileResponse
    {
        public Guid Id { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Power { get; set; }
        public CarType CarType { get; set; }
    }
}
