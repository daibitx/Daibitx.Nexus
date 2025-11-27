using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Daibitx.Extension.Modularize.Abstractons
{
    public abstract class StartupBase : IStartup
    {
        protected StartupBase()
        {
        }



        public abstract void ConfigurationService(IConfiguration configuration, IServiceCollection services);
        public abstract void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);
    }
}
