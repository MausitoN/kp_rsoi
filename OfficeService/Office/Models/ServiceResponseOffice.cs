using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Models
{
    public class ServiceResponseOffice
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseOffice(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseOffice(object result) : this(result, 200, null) { }
        public ServiceResponseOffice(int code, string message) : this(null, code, message) { }
        public ServiceResponseOffice(int code, object result) : this(result, code, null) { }
        public ServiceResponseOffice(int code) : this(null, code, null) { }
    }
}
