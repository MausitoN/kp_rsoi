using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Models
{
    public class RentOffice
    {
        public Guid Id { get; set; }
        public string Location { get; set; }

        public virtual List<AvailableCar> AvailableCars { get; set; }
    }
}
