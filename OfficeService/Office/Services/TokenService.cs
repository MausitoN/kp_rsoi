using OfficeService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeService.Services
{
    public class TokenService : ITokenService
    {
        private string _carServiceToken;

        public TokenService()
        {
            _carServiceToken = string.Empty;
        }
        public string GetCarServiceToken()
        {
            return _carServiceToken;
        }

        public void SetCarServiceToken(string token)
        {
            _carServiceToken = token;
        }
    }
}
