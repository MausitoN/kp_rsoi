using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GatewayService.Interfaces
{
    public enum AuthResult
    {
        Authorized,
        Unauthorized,
        Forbidden
    }

    public interface IAuthService
    {
        Task<AuthResult> ValidateTokenAsync(string token);
    }
}
