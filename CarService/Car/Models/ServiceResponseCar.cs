using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarService.Models
{
    public class ServiceResponseCar
    {
        public object Result { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        private ServiceResponseCar(object result, int code, string message)
        {
            Result = result;
            StatusCode = code;
            Message = message;
        }

        public ServiceResponseCar(object result) : this(result, 200, null) { }
        public ServiceResponseCar(int code, string message) : this(null, code, message) { }
        public ServiceResponseCar(int code, object result) : this(result, code, null) { }
        public ServiceResponseCar(int code, string message, object result) : this(result, code, message) { }
        public ServiceResponseCar(int code) : this(null, code, null) { }
    }
}
