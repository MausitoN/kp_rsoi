using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Models
{
    public class ReturnCarRequest
    {
        public Guid OfficeUid { get; set; }
        public Guid CarUid { get; set; }
        public string RegistrationNumber { get; set; }

    }
}
