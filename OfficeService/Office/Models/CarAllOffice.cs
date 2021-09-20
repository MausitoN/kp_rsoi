using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Models
{
    public class CarAllOffice
    {
        public Guid CarUid { get; set; }
        public Guid OfficeUid { get; set; }
        public string Location { get; set; }
        public long RegistrationNumber { get; set; }
    }
}
