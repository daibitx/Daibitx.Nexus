using AgileConfig.Client;

namespace Daibitx.Nuxus.Core
{
    public class DynamicConfigProvider
    {
        private readonly IConfigClient _configClient;
        private readonly YarpDynamicConfigLoader _loader;

        private InMemoryConfig _currentConfig;
        private CancellationTokenSource _cts = new();
       


    }
}
