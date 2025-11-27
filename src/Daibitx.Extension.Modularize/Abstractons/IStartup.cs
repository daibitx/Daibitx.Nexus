using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Daibitx.Extension.Modularize.Abstractons
{
    internal interface IStartup
    {

        void ConfigurationService(IConfiguration configuration, IServiceCollection services);

        void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider);
    }
}
