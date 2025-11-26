using AgileConfig.Client;
using System.Text.Json;
using Yarp.ReverseProxy.Configuration;

namespace Daibitx.Nuxus.Core
{
    public class DynamicConfigManager
    {
        private readonly IServiceProvider _serviceProvider;
        private object _lock = new object();
        public DynamicConfigManager(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _serviceProvider.GetRequiredService<IConfigClient>().ReLoaded += ReloadConfig;

        }

        public (IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters) LoadFromConfig()
        {
            using var scope = _serviceProvider.CreateScope();
            var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var _logger = scope.ServiceProvider.GetRequiredService<ILogger<DynamicConfigManager>>();
            var _configClient = scope.ServiceProvider.GetRequiredService<IConfigClient>();

            string sectionName = _configuration["YarpSectionKey"];
            if (string.IsNullOrEmpty(sectionName))
            {
                _logger.LogError("YarpSectionKey is not set in appsettings.json");
                return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
            }

            var configJson = _configClient[sectionName];
            if (string.IsNullOrEmpty(configJson))
            {
                _logger.LogError("Yarp section is not found in config server.");
                return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
            }

            try
            {
                var document = JsonDocument.Parse(configJson);

                var routes = new List<RouteConfig>();
                var clusters = new List<ClusterConfig>();

                if (document.RootElement.TryGetProperty("Routes", out var routesElem))
                {
                    foreach (var r in routesElem.EnumerateArray())
                    {
                        try
                        {
                            var route = JsonSerializer.Deserialize<RouteConfig>(r.GetRawText());
                            if (route != null) routes.Add(route);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize RouteConfig: {Json}", r.GetRawText());
                        }
                    }
                }

                if (document.RootElement.TryGetProperty("Clusters", out var clusterElem))
                {
                    foreach (var c in clusterElem.EnumerateArray())
                    {
                        try
                        {
                            var cluster = JsonSerializer.Deserialize<ClusterConfig>(c.GetRawText());
                            if (cluster != null) clusters.Add(cluster);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to deserialize ClusterConfig: {Json}", c.GetRawText());
                        }
                    }
                }

                return (routes, clusters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse YARP config JSON");
                return (Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>());
            }
        }

        private void ReloadConfig(ConfigReloadedArgs args)
        {
            lock (_lock)
            {
                using var scope = _serviceProvider.CreateScope();
                var _logger = scope.ServiceProvider.GetRequiredService<ILogger<DynamicConfigManager>>();
                var _configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var inmemoryConfigProvider = scope.ServiceProvider.GetRequiredService<InMemoryConfigProvider>();
                try
                {
                    string sectionName = _configuration["YarpSectionKey"];

                    if (string.IsNullOrEmpty(sectionName))
                    {
                        _logger.LogError("YarpSectionKey is not set in appsettings.json");
                        return;
                    }
                    var configJson = args.NewConfigs[sectionName];

                    var document = JsonDocument.Parse(configJson);

                    var routes = new List<RouteConfig>();

                    var clusters = new List<ClusterConfig>();

                    if (document.RootElement.TryGetProperty("Routes", out var routesElem))
                    {
                        foreach (var r in routesElem.EnumerateArray())
                        {
                            try
                            {
                                var route = JsonSerializer.Deserialize<RouteConfig>(r.GetRawText());
                                if (route != null) routes.Add(route);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to deserialize RouteConfig: {Json}", r.GetRawText());
                            }
                        }
                    }

                    if (document.RootElement.TryGetProperty("Clusters", out var clusterElem))
                    {
                        foreach (var c in clusterElem.EnumerateArray())
                        {
                            try
                            {
                                var cluster = JsonSerializer.Deserialize<ClusterConfig>(c.GetRawText());
                                if (cluster != null) clusters.Add(cluster);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to deserialize ClusterConfig: {Json}", c.GetRawText());
                            }
                        }
                    }

                    inmemoryConfigProvider.Update(routes, clusters);

                    _logger.LogInformation("YARP config reloaded, routes: {@Routes}, clusters: {@Clusters}", routes, clusters);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse YARP config JSON");
                    return;
                }
            }
        }
      
    }
}
