using Daibitx.Extension.Modularize.Abstractons;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daibitx.Extension.Modularize
{
    internal class ModulePipeline
    {
        private readonly ILogger<ModulePipeline> _logger;
        private readonly ModuleDiscoverer _moduleDiscoverer;
        private readonly ModuleLoader _moduleLoader;
        private readonly List<ModuleDescriptor> modules;
        private readonly IHostBuilder _hostBuilder;
        private readonly IConfiguration _configuration;
        public ModulePipeline(ILogger<ModulePipeline> logger,
                             ModuleDiscoverer moduleDiscoverer,
                             ModuleLoader moduleLoader,
                             IHostBuilder hostBuilder,
                             IConfiguration configuration)
        {
            _logger = logger;
            _moduleDiscoverer = moduleDiscoverer;
            _moduleLoader = moduleLoader;
            _hostBuilder = hostBuilder;
            modules = new List<ModuleDescriptor>();
            _hostBuilder = hostBuilder;
            _configuration = configuration;
        }

        public void Build(IServiceCollection services)
        {
            _logger.LogInformation("Initializing module pipeline...");
            var moduleDescriptors = _moduleDiscoverer.Discover();
            foreach (var moduleDescriptor in moduleDescriptors)
            {
                _logger.LogInformation($"Loading module {moduleDescriptor.AssemblyName}...");
                var module = _moduleLoader.LoadModuleAssembly(moduleDescriptor);
                if (module != null)
                    modules.Add(moduleDescriptor);
            }

            foreach (var module in modules)
            {
                LoadModuleConfiguration(module);

                module.Startup.ConfigurationService(_configuration, services);

                var partManager = services.First(s => s.ServiceType == typeof(ApplicationPartManager)).ImplementationInstance as ApplicationPartManager;
                partManager.ApplicationParts.Add(new AssemblyPart(module.Assembly));
            }
        }

        public void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            foreach (var module in modules)
            {
                module.Startup.Configure(builder, routes, serviceProvider);
            }
        }

        private void LoadModuleConfiguration(ModuleDescriptor descriptor)
        {
            if (descriptor.ConfigPath == null || !File.Exists(descriptor.ConfigPath))
            {
                _logger.LogInformation($"No configuration file found for module {descriptor.AssemblyName}.");
                return;
            }
            _hostBuilder.ConfigureAppConfiguration((_, cfb) =>
            {
                cfb.Add(new ModuleConfigSource(descriptor.ConfigPath, descriptor.AssemblyName));
            });

            _logger.LogInformation($"Configuration for module {descriptor.AssemblyName} loaded.");
        }


    }
}
