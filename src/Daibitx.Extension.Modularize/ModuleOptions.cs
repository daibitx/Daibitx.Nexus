using System.Collections.Generic;

namespace Daibitx.Extension.Modularize
{
    public class ModuleOptions
    {
        public string ModulesPath { get; set; }
        internal List<ModuleDescriptor> Modules { get; } = new();
    }
}
