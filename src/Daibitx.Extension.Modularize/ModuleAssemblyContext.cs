using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Daibitx.Extension.Modularize
{
    public class ModuleAssemblyContext : AssemblyLoadContext
    {
        private readonly string _modulePath;
        private readonly Dictionary<string, Assembly> _loadedAssemblies = new();

        public ModuleAssemblyContext(string modulePath)
            : base(isCollectible: true)
        {
            _modulePath = modulePath;
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (_loadedAssemblies.TryGetValue(assemblyName.Name, out var existing))
                return existing;

            string assemblyFileName = assemblyName.Name + ".dll";

            string fullPath = Path.Combine(_modulePath, assemblyFileName);

            if (File.Exists(fullPath))
            {
                var asm = LoadFromAssemblyPath(fullPath);
                _loadedAssemblies[assemblyName.Name] = asm;
                return asm;
            }

            if (Default.Assemblies.Any(a => a.GetName().Name == assemblyName.Name))
            {
                var asm = Default.LoadFromAssemblyName(assemblyName);
                _loadedAssemblies[assemblyName.Name] = asm;
                return asm;
            }
            try
            {
                return Default.LoadFromAssemblyName(assemblyName);
            }
            catch
            {
                return null;
            }
        }
        protected override nint LoadUnmanagedDll(string unmanagedDllName)
        {
            string path = Path.Combine(_modulePath, GetDllName(unmanagedDllName));
            if (File.Exists(path))
            {
                return LoadUnmanagedDllFromPath(path);
            }
            return base.LoadUnmanagedDll(unmanagedDllName);
        }

        private string GetDllName(string name)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"{name}.dll"
                : RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? $"lib{name}.so"
                    : $"lib{name}.dylib";
        }
    }
}
