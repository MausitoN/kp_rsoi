using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SessionService.Interfaces;
using SessionService.Models;
using ShareService.Configurations;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SessionService.Sevices
{
    public class AuthService: IAuthService
    {
        private readonly SecretOptions _secretKeys;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AuthService(IOptions<SecretOptions> secretKeys)
        {
            _secretKeys = secretKeys?.Value ?? throw new ArgumentNullException(nameof(secretKeys));
        }

        public TokenModel GetAccessToken(IEnumerable<Claim> claims)
        {
            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(_secretKeys.PrivateKey), out int _);

            var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);

            var jwtToken = new JwtSecurityToken(
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddMinutes(_secretKeys.AccessExpirationInMin),
                signingCredentials: credentials,
                claims: claims
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var res = new TokenModel()
            {
                Token = accessToken
            };

            //var resultSign = await _signInManager.PasswordSignInAsync();

            return res;
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(_secretKeys.PublicKey), out int _);

            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                RequireSignedTokens = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (SecurityTokenExpiredException)
            {
                Console.WriteLine("Validation token failed. Token expired");
                throw;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
