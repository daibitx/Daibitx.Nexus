namespace Daibitx.Nuxus.Hmac
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    namespace Daibitx.Security.Hmac
    {
        public static class HmacExtensions
        {
            public static IServiceCollection AddHmac(this IServiceCollection services, IConfiguration config)
            {
                services.Configure<HmacOptions>(config.GetSection("Hmac"));
                services.AddSingleton<HmacValidator>();
                services.AddSingleton<HmacGenerator>();
                return services;
            }

            public static IApplicationBuilder UseHmacValidation(this IApplicationBuilder app)
            {
                return app.UseMiddleware<HmacValidationMiddleware>();
            }
        }
    }

}
