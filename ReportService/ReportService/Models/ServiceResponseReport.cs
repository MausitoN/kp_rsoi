using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportService.Models
{
    public class ServiceResponseReport
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseReport(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseReport(object result) : this(result, 200, null) { }
        public ServiceResponseReport(int code, string message) : this(null, code, message) { }
        public ServiceResponseReport(int code, object result) : this(result, code, null) { }
        public ServiceResponseReport(int code) : this(null, code, null) { }
    }
}
