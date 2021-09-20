using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Models
{
    public class AddCarRequest
    {
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Power { get; set; }
        public CarType CarType { get; set; }
    }
}
