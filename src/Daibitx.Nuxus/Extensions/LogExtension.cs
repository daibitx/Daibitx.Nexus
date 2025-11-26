using Serilog;
using Serilog.Events;

namespace Daibitx.Nuxus.Extensions
{
    public static class LogExtension
    {
        public static IServiceCollection AddSoryoLogging(this IServiceCollection services)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logsDirectory = Path.Combine(Directory.GetParent(baseDirectory).FullName, "..", "log");

            Directory.CreateDirectory(logsDirectory);

            string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
            string datePath = Path.Combine(logsDirectory, dateFolder);
            Directory.CreateDirectory(datePath);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Information)
                    .WriteTo.Async(a => a.File(Path.Combine(datePath, "Information-.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.Async(a => a.File(Path.Combine(datePath, "Warning-.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Error)
                    .WriteTo.Async(a => a.File(Path.Combine(datePath, "Error-.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Fatal)
                    .WriteTo.Async(a => a.File(Path.Combine(datePath, "Fatal-.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")))
                .CreateLogger();
            return services;
        }
    }

}
