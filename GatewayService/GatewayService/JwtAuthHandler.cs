using GatewayService.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace GatewayService
{
    public class JwtAuthHandler : AuthenticationHandler<JwtBearerOptions>
    {
        private readonly IAuthService _authService;

        public JwtAuthHandler(
            IOptionsMonitor<JwtBearerOptions> options,
            IAuthService authService,
            UrlEncoder encoder,
            ILoggerFactory loggerFactory,
            ISystemClock clock)
            : base(options, loggerFactory, encoder, clock)
        {
            _authService = authService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return AuthenticateResult.NoResult();

            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var token = Uri.EscapeDataString(authHeader.Parameter);

                if (string.IsNullOrEmpty(token))
                    return AuthenticateResult.Fail("Missing JWT token");


                var authResult = await _authService.ValidateTokenAsync(token);
                if (authResult == AuthResult.Authorized)
                {
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadJwtToken(token);
                    var identity = new ClaimsIdentity(jsonToken.Claims, Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                else if (authResult == AuthResult.Forbidden)
                {
                    var identity = new ClaimsIdentity(new List<Claim>(), Scheme.Name);
                    var principal = new ClaimsPrincipal(identity);
                    var ticket = new AuthenticationTicket(principal, Scheme.Name);
                    return AuthenticateResult.Success(ticket);
                }
                else
                    return AuthenticateResult.Fail("Invalid token");
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }
}