using Daibitx.Extension.Modularize.Abstractons;
using System.Reflection;

namespace Daibitx.Extension.Modularize
{
    internal class ModuleDescriptor
    {
        public string AssemblyName { get; set; }
        public string DllPath { get; set; }
        public string ConfigPath { get; set; }
        public string AssemblyContextPath { get; set; }
        public IStartup Startup { get; set; }
        public Assembly Assembly { get; set; }
        public ModuleAssemblyContext AssemblyContext { get; set; }
    }
}
