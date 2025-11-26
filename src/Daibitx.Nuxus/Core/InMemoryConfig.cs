using Microsoft.Extensions.Primitives;
using Yarp.ReverseProxy.Configuration;

namespace Daibitx.Nuxus.Core
{
    public class InMemoryConfig : IProxyConfig
    {
        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, CancellationToken token)
        {
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(token);
        }

        public IReadOnlyList<RouteConfig> Routes { get; }
        public IReadOnlyList<ClusterConfig> Clusters { get; }
        public IChangeToken ChangeToken { get; }
    }
}
