using GatewayService.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GatewayService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<AuthResult> ValidateTokenAsync(string token)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var response = await httpClient.PostAsync($"{_configuration["SessionServiceUri"]}/verify", null);
                
                if (response.StatusCode == HttpStatusCode.OK)
                    return AuthResult.Authorized;
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                    return AuthResult.Forbidden;
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    return AuthResult.Unauthorized;
                else
                    throw new Exception("SessionService unawailable");
            }
        }
    }
}
