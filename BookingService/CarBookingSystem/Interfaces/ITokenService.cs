using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Interfaces
{
    public interface ITokenService
    {
        string GetCarServiceToken();
        void SetCarServiceToken(string token);
        string GetOfficeServiceToken();
        void SetOfficeServiceToken(string token);
        string GetPaymentServiceToken();
        void SetPaymentServiceToken(string token);
    }
}
