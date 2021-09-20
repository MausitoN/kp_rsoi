using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Interfaces
{
    public interface ITokenService
    {
        string GetCarServiceToken();
        void SetCarServiceToken(string token);
        string GetOfficeServiceToken();
        void SetOfficeServiceToken(string token);
        string GetPaymentServiceToken();
        void SetPaymentServiceToken(string token);
        string GetBookingServiceToken();
        void SetBookingServiceToken(string token);
        string GetSessionServiceToken();
        void SetSessionServiceToken(string token);
        string GetReportServiceToken();
        void SetReportServiceToken(string token);
    }
}
