using SessionService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SessionService.Interfaces
{
    public interface IAuthService
    {
        TokenModel GetAccessToken(IEnumerable<Claim> claims);
        bool ValidateToken(string token);
    }
}
