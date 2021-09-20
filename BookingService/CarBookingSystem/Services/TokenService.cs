using BookingService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookingService.Services
{
    public class TokenService : ITokenService
    {
        private string _carServiceToken;
        private string _officeServiceToken;
        private string _paymentServiceToken;

        public TokenService()
        {
            _carServiceToken = string.Empty;
            _officeServiceToken = string.Empty;
            _paymentServiceToken = string.Empty;
        }
        public string GetCarServiceToken()
        {
            return _carServiceToken;
        }

        public void SetCarServiceToken(string token)
        {
            _carServiceToken = token;
        }

        public string GetOfficeServiceToken()
        {
            return _officeServiceToken;
        }

        public void SetOfficeServiceToken(string token)
        {
            _officeServiceToken = token;
        }

        public string GetPaymentServiceToken()
        {
            return _paymentServiceToken;
        }

        public void SetPaymentServiceToken(string token)
        {
            _paymentServiceToken = token;
        }
    }
}
