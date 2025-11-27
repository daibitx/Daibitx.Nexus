using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace Daibitx.Extension.Modularize.Core
{
    public class ModuleAssemblyContext : AssemblyLoadContext
    {
        private readonly string _modulePath;

        public ModuleAssemblyContext(string modulePath)
            : base(isCollectible: true)
        {
            _modulePath = modulePath;
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyFileName = assemblyName.Name + ".dll";

            string fullPath = Path.Combine(_modulePath, assemblyFileName);

            if (File.Exists(fullPath))
            {
                return LoadFromAssemblyPath(fullPath);
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

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
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
                : (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? $"lib{name}.so"
                    : $"lib{name}.dylib");
        }
    }
}
