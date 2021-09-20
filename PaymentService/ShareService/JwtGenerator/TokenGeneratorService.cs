using Microsoft.Extensions.Options;
using ShareService.Configurations;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;


namespace ShareService.JwtGenerator
{
    public class TokenGeneratorService : ITokenGeneratorService
    {
        private readonly RSA _rsa;
        private readonly SecretOptions _secretOptions;

        public TokenGeneratorService(IOptions<SecretOptions> secretOptions)
        {
            _secretOptions = secretOptions.Value;
            _rsa = RSA.Create();
        }

        public string GetToken()
        {
            var key = _secretOptions.PrivateKey;
            _rsa.ImportRSAPrivateKey(Convert.FromBase64String(key), out int _);

            var credentials = new SigningCredentials(new RsaSecurityKey(_rsa), SecurityAlgorithms.RsaSha256);

            var jwtToken = new JwtSecurityToken( // знаем чем заменить
                notBefore: DateTime.UtcNow,
                expires: DateTime.Now.AddMinutes(_secretOptions.AccessExpirationInMin),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtToken); // знаем чем заменить
        }
    }
}
