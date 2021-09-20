using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SessionService.Models
{
    public class ServiceResponseSession
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseSession(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseSession(object result) : this(result, 200, null) { }
        public ServiceResponseSession(int code, string message) : this(null, code, message) { }
        public ServiceResponseSession(int code, object result) : this(result, code, null) { }
        public ServiceResponseSession(int code) : this(null, code, null) { }
    }
}
