using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Daibitx.Extension.Modularize
{
    public static class HostServiceExtension
    {
        public static void ConfigureModulesService(this WebApplicationBuilder application, Action<ModuleOptions> moduleOptions)
        {
            var options = new ModuleOptions();
            moduleOptions(options);
            var logger = GetLogger<ModulePipeline>(application.Services);
            var moduleDiscoverer = new ModuleDiscoverer(options.ModulesPath);
            var moduleLoader = new ModuleLoader(logger);
            var moduleDescriptors = moduleDiscoverer.Discover();
            foreach (var moduleDescriptor in moduleDescriptors)
            {
                logger.LogInformation($"Loading module {moduleDescriptor.AssemblyName}...");
                var module = moduleLoader.LoadModuleAssembly(moduleDescriptor);
                if (module != null)
                    options.Modules.Add(moduleDescriptor);
            }
            var pipelineBuilder = new ModulePipeline();
            pipelineBuilder.Build(application.Services, logger, options, application.Host, application.Configuration);
            application.Services.AddSingleton(options);
        }

        public static void ConfigureModules(this IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var modulePipeline = new ModulePipeline();
            var options = serviceProvider.GetRequiredService<ModuleOptions>();
            modulePipeline.Configure(builder, routes, serviceProvider, options);
        }

        private static ILogger GetLogger<T>(this IServiceCollection services)
        {
            ILoggerFactory loggerFactory;
            var loggerFactoryDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ILoggerFactory));
            if (loggerFactoryDescriptor?.ImplementationInstance is ILoggerFactory)
            {
                loggerFactory = (ILoggerFactory)loggerFactoryDescriptor.ImplementationInstance;
            }
            else
            {
                var tempServiceProvider = services.BuildServiceProvider();
                loggerFactory = tempServiceProvider.GetRequiredService<ILoggerFactory>();
            }
            return loggerFactory.CreateLogger<T>();

        }

    }
}
