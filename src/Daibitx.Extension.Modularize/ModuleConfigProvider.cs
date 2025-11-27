using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Daibitx.Extension.Modularize
{
    internal class ModuleConfigProvider : IConfigurationProvider
    {
        private readonly IConfigurationProvider _innerProvider;
        private readonly string _prefix;

        public ModuleConfigProvider(IConfigurationProvider innerProvider, string prefix)
        {
            _innerProvider = innerProvider;
            _prefix = prefix;
        }

        public void Load() => _innerProvider.Load();

        public IChangeToken GetReloadToken() => _innerProvider.GetReloadToken();

        public bool TryGet(string key, out string value)
        {
            if (key.StartsWith(_prefix + ":", StringComparison.OrdinalIgnoreCase))
            {
                var innerKey = key.Substring(_prefix.Length + 1);
                return _innerProvider.TryGet(innerKey, out value);
            }
            value = null;
            return false;
        }

        public void Set(string key, string value)
        {
            if (key.StartsWith(_prefix + ":", StringComparison.OrdinalIgnoreCase))
            {
                var innerKey = key.Substring(_prefix.Length + 1);
                _innerProvider.Set(innerKey, value);
            }
        }

        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
        {
            if (parentPath == _prefix || (parentPath?.StartsWith(_prefix + ":") ?? false))
            {
                string innerParent = parentPath == _prefix ? null : parentPath.Substring(_prefix.Length + 1);
                var innerKeys = _innerProvider.GetChildKeys(earlierKeys, innerParent);
                return innerKeys.Select(k => $"{_prefix}:{k}");
            }
            return Enumerable.Empty<string>();
        }
    }
}
