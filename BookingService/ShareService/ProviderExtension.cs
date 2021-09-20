using Microsoft.Extensions.DependencyInjection;
using ShareService.JwtGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareService
{
    public static class ProviderExtension
    {
        public static IServiceCollection AddTokenGenerator(this IServiceCollection services)
        {
            services.AddSingleton<ITokenGeneratorService, TokenGeneratorService>();
            return services;
        }
    }
}
