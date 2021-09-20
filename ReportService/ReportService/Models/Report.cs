using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Models
{
    public class Report
    {
        public long Id { get; set; }
        public string Model { get; set; }
        public string Location { get; set; }
    }
}
