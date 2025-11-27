using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace Daibitx.Extension.Modularize
{
    internal class ModulePipeline
    {
        public void Build(IServiceCollection services, ILogger logger, ModuleOptions options,
                            IHostBuilder hostBuilder, IConfiguration configuration)
        {
            logger.LogInformation("Initializing module pipeline...");
            foreach (var module in options.Modules)
            {
                LoadModuleConfiguration(module, hostBuilder, logger);

                module.Startup.ConfigurationService(configuration, services);

                var partManager = services.First(s => s.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance as ApplicationPartManager;
                partManager.ApplicationParts.Add(new AssemblyPart(module.Assembly));
            }
        }

        public void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider, ModuleOptions options)
        {
            foreach (var module in options.Modules)
            {
                module.Startup.Configure(builder, routes, serviceProvider);
            }
        }

        private void LoadModuleConfiguration(ModuleDescriptor descriptor, IHostBuilder _hostBuilder, ILogger logger)
        {
            if (descriptor.ConfigPath == null || !File.Exists(descriptor.ConfigPath))
            {
                logger.LogInformation($"No configuration file found for module {descriptor.AssemblyName}.");
                return;
            }
            _hostBuilder.ConfigureAppConfiguration((_, cfb) =>
            {
                cfb.Add(new ModuleConfigSource(descriptor.ConfigPath, descriptor.AssemblyName));
            });

            logger.LogInformation($"Configuration for module {descriptor.AssemblyName} loaded.");
        }


    }
}
