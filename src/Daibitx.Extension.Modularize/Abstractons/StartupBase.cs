using Daibitx.Extension.Modularize.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Daibitx.Extension.Modularize.Abstractons
{
    public abstract class StartupBase : IStartup
    {
        internal ModularizeMetaData MetaData { get; set; }
        protected StartupBase()
        {
            MetaData = new ModularizeMetaData();
        }
        public void Configuration(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
        }

        public void ConfigurationService(IConfiguration configuration, IServiceCollection services)
        {
        }
    }
}
