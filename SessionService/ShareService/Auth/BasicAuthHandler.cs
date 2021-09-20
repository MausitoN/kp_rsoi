using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareService.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ShareService.Auth
{
    public class BasicAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ServicesCredentialsOptions _credentials;

        public BasicAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            IOptionsMonitor<ServicesCredentialsOptions> credentials,
            UrlEncoder encoder,
            ILoggerFactory loggerFactory,
            ISystemClock clock)
            : base(options, loggerFactory, encoder, clock)
        {
            _credentials = credentials?.CurrentValue ?? throw new ArgumentNullException(nameof(credentials));
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                return Task.FromResult(AuthenticateResult.NoResult());

            if (!Request.Headers.ContainsKey("Authorization"))
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var userName = credentials[0];
                var password = credentials[1];
                if (!CheckCredentials(userName, password))
                    return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));

                var claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, userName) };
                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }
        }

        private bool CheckCredentials(string userName, string password)
        {
            return _credentials.Credentials.Any(c => c.ClientId == userName && c.ClientSecret == password);
        }
    }
}
