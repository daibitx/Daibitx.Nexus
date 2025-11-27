using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Daibitx.Extension.Modularize
{
    internal class ModuleDiscoverer
    {
        private readonly string _modulesRootPath;

        internal ModuleDiscoverer(string modulesRootPath)
        {
            _modulesRootPath = string.IsNullOrEmpty(modulesRootPath)
                ? Path.Combine(Directory.GetCurrentDirectory(), "plugins")
                : Path.GetFullPath(modulesRootPath);
        }

        internal IEnumerable<ModuleDescriptor> Discover()
        {
            if (!Directory.Exists(_modulesRootPath))
            {
                return Enumerable.Empty<ModuleDescriptor>();
            }
            var moduleNames = Directory.GetDirectories(Directory.GetCurrentDirectory())
                                  .Select(Path.GetFileName)
                                  .ToList();
            var moduleDescriptors = new List<ModuleDescriptor>();
            foreach (var moduleName in moduleNames)
            {
                var moduleDescriptor = DiscoverModule(moduleName);
                if (moduleDescriptor != null)
                {
                    moduleDescriptors.Add(moduleDescriptor);
                }
            }
            return moduleDescriptors;

        }


        internal ModuleDescriptor DiscoverModule(string moduleName)
        {
            var modulePath = Path.Combine(_modulesRootPath, moduleName);
            var mainDllPath = Path.Combine(modulePath, moduleName + ".dll");
            if (!File.Exists(mainDllPath))
            {
                return null;
            }
            var moduleDescriptor = new ModuleDescriptor();
            moduleDescriptor.AssemblyName = moduleName;
            moduleDescriptor.DllPath = mainDllPath;
            moduleDescriptor.ConfigPath = Path.Combine(_modulesRootPath, moduleName, ".json");
            moduleDescriptor.AssemblyContextPath = modulePath;
            return moduleDescriptor;
        }

    }
}
