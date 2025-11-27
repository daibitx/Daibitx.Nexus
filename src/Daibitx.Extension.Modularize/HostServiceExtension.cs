using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Daibitx.Extension.Modularize
{
    public static class HostServiceExtension
    {
        public static void ConfigureModulesService(this WebApplicationBuilder application, Action<string> rootConfig)
        {
            ILoggerFactory loggerFactory;
            var loggerFactoryDescriptor = application.Services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory));
            if (loggerFactoryDescriptor?.ImplementationInstance is ILoggerFactory)
            {
                loggerFactory = (ILoggerFactory)loggerFactoryDescriptor.ImplementationInstance;
            }
            else
            {
                var tempServiceProvider = application.Services.BuildServiceProvider();
                loggerFactory = tempServiceProvider.GetRequiredService<ILoggerFactory>();
            }
            var rootPath = string.Empty;
            rootConfig(rootPath);
            var moduleDiscoverer = new ModuleDiscoverer(rootPath);
            var moduleLoader = new ModuleLoader(loggerFactory.CreateLogger<ModuleLoader>());
            var pipelineBuilder = new ModulePipeline(loggerFactory.CreateLogger<ModulePipeline>(),
                                                     moduleDiscoverer, moduleLoader,
                                                     application.Host, application.Configuration);
            pipelineBuilder.Build(application.Services);

            application.Services.AddSingleton(pipelineBuilder);
        }

        public static void ConfigureModules(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            serviceProvider.GetRequiredService<ModulePipeline>().Configure(builder, routes, serviceProvider);
        }
    }
}
