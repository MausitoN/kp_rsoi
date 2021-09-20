using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentService.Models
{
    public class ServiceResponsePayment
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponsePayment(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponsePayment(object result) : this(result, 200, null) { }
        public ServiceResponsePayment(int code, string message) : this(null, code, message) { }
        public ServiceResponsePayment(int code, object result) : this(result, code, null) { }
        public ServiceResponsePayment(int code) : this(null, code, null) { }
    }
}
