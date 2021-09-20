using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Models
{
    public class ServiceResponseGateway
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseGateway(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseGateway(object result) : this(result, 200, null) { }
        public ServiceResponseGateway(int code, string message) : this(null, code, message) { }
        public ServiceResponseGateway(int code, object result) : this(result, code, null) { }
        public ServiceResponseGateway(int code) : this(null, code, null) { }
    }
}
