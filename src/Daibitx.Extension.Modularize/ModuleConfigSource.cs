using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Daibitx.Extension.Modularize
{
    internal class ModuleConfigSource : IConfigurationSource
    {
        private readonly string _configPath;
        private readonly string _moduleName;
        public ModuleConfigSource(string configPath, string moduleName)
        {
            _configPath = configPath;
            _moduleName = moduleName;
        }
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var jsonSource = new JsonConfigurationSource
            {
                Path = _configPath,
                Optional = true,
                ReloadOnChange = true,
                FileProvider = null
            };
            var innerProvider = jsonSource.Build(builder);
            return new ModuleConfigProvider(innerProvider, _moduleName);
        }
    }
}
