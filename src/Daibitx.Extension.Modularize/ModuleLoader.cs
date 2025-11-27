using Daibitx.Extension.Modularize.Abstractons;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;

namespace Daibitx.Extension.Modularize
{
    internal class ModuleLoader
    {
        private readonly ILogger _logger;

        internal ModuleLoader(ILogger logger)
        {
            _logger = logger;
        }

        internal ModuleDescriptor LoadModuleAssembly(ModuleDescriptor descriptor)
        {
            try
            {
                var loadContext = new ModuleAssemblyContext(descriptor.AssemblyContextPath);
                var assembly = loadContext.LoadFromAssemblyPath(descriptor.DllPath);
                descriptor.Assembly = assembly;
                descriptor.AssemblyContext = loadContext;
                var startupType = FindStartup(assembly);
                var startup = Activator.CreateInstance(startupType) as IStartup;
                if (startup == null)
                {
                    throw new InvalidOperationException("No Startup class found in module assembly.");
                }
                descriptor.Startup = startup;
                return descriptor;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load module assembly {0}", descriptor.AssemblyName);
                return null;
            }
        }

        private Type FindStartup(Assembly assembly)
        {
            var startupType = assembly.GetTypes().FirstOrDefault(t => typeof(IStartup).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract);
            if (startupType == null)
            {
                throw new InvalidOperationException("No Startup class found in module assembly.");
            }
            return startupType;

        }
    }
}
