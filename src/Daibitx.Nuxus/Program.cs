using Daibitx.Nuxus.Hmac;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Daibitx.Nuxus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHealthChecks();

            builder.Host.UseAgileConfig();

            builder.Services.AddLogging();

            builder.Host.UseSerilog();

            builder.Services.AddRateLimiter(options =>
            {
                var config = builder.Configuration.GetSection("RateLimit");

                int permitLimit = config.GetValue<int>("PermitLimit", 1000);
                int window = config.GetValue<int>("Window", 10);
                int segmentCount = config.GetValue<int>("SegmentsPerWindow", 10);
                int queueLimit = config.GetValue<int>("QueueLimit", 0);

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: ip,
                        factory: _ => new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromSeconds(window),
                            SegmentsPerWindow = segmentCount,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = queueLimit
                        }
                    );
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddReverseProxy().LoadFromMemory(new List<RouteConfig>(), new List<ClusterConfig>())
                .AddTransforms(transformBuilderContext =>
                {
                    transformBuilderContext.AddRequestTransform(async transformContext =>
                    {
                        var req = transformContext.HttpContext.Request;
                        var config = transformContext.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                        string secret = config.GetValue<string>("Hmac:SecretKey") ?? "";

                        string body = "";
                        if (req.ContentLength > 0)
                        {
                            req.EnableBuffering();
                            using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                            body = await reader.ReadToEndAsync();
                            req.Body.Position = 0;
                        }

                        string bodyHash = string.IsNullOrEmpty(body)
                            ? ""
                            : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLower();

                        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                        string method = req.Method;
                        string path = req.Path;

                        string message = $"{method}:{path}:{timestamp}:{bodyHash}";
                        string signature = HmacValidator.ComputeHmac(secret, message);

                        transformContext.ProxyRequest.Headers.Add("X-GW-Timestamp", timestamp);
                        transformContext.ProxyRequest.Headers.Add("X-GW-BodyHash", bodyHash);
                        transformContext.ProxyRequest.Headers.Add("X-GW-Signature", signature);
                    });
                });

            var app = builder.Build();

            app.UseRateLimiter();

            app.MapReverseProxy();

            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
