using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ShareService.Auth;
using ShareService.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ShareService
{
    public static class AuthProvider
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, 
                                                    IConfiguration configuration)
        {
            services.Configure<SecretOptions>(configuration.GetSection("SecurityKeys"));

            services.AddSingleton<RsaSecurityKey>(provider => {
                RSA rsa = RSA.Create();
                var key = configuration["SecurityKeys:PublicKey"];
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key), out int _);

                return new RsaSecurityKey(rsa);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    SecurityKey rsa = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = rsa,
                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            // Ensure we always have an error and error description.
                            if (string.IsNullOrEmpty(context.Error))
                                context.Error = "InvalidToken";
                            if (string.IsNullOrEmpty(context.ErrorDescription))
                                context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                            // Add some extra context for expired tokens.
                            if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
                            }

                            return context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                error = context.Error,
                                errorDescription = context.ErrorDescription
                            }));
                        }
                    };
                });

            return services;
        }

        public static IServiceCollection AddJwtAndBasicAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<SecretOptions>(configuration.GetSection("SecurityKeys"));
            services.Configure<ServicesCredentialsOptions>(configuration.GetSection("ServiceCredentials"));
            //services.Configure<ServiceCredentials>(configuration.GetSection("ServiceCredentials"));
            //services.Configure<ServicesCredentialsOptions>(configuration.GetSection("ServicesCredentials"));

            services.AddSingleton<RsaSecurityKey>(provider => {
                RSA rsa = RSA.Create();
                var key = configuration["SecurityKeys:PublicKey"];
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(key), out int _);

                return new RsaSecurityKey(rsa);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.IncludeErrorDetails = true;
                    SecurityKey rsa = services.BuildServiceProvider().GetRequiredService<RsaSecurityKey>();
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        IssuerSigningKey = rsa,
                        RequireSignedTokens = true,
                        RequireExpirationTime = true,
                        ValidateIssuerSigningKey = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents()
                    {
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/json";

                            // Ensure we always have an error and error description.
                            if (string.IsNullOrEmpty(context.Error))
                                context.Error = "InvalidToken";
                            if (string.IsNullOrEmpty(context.ErrorDescription))
                                context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                            // Add some extra context for expired tokens.
                            if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                            {
                                var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
                            }

                            return context.Response.WriteAsync(JsonSerializer.Serialize(new
                            {
                                error = context.Error,
                                errorDescription = context.ErrorDescription
                            }));
                        }
                    };
                })
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("Basic", null);

            return services;
        }
    }
}
