using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Models
{
    public class ServiceResponseBooking
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseBooking(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseBooking(object result) : this(result, 200, null) { }
        public ServiceResponseBooking(int code, string message) : this(null, code, message) { }
        public ServiceResponseBooking(int code, object result) : this(result, code, null) { }
        public ServiceResponseBooking(int code) : this(null, code, null) { }
    }
}
