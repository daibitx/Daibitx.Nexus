using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;

namespace Daibitx.Nuxus.Hmac
{
    public class HmacValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HmacValidator _validator;

        public HmacValidationMiddleware(RequestDelegate next, HmacValidator validator)
        {
            _next = next;
            _validator = validator;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var req = context.Request;

            string sig = req.Headers["X-GW-Signature"];
            string ts = req.Headers["X-GW-Timestamp"];
            string reqBodyHash = req.Headers["X-GW-BodyHash"];

            string body = "";
            string realBodyHash = "";

            if (req.ContentLength > 0)
            {
                req.EnableBuffering();
                using var reader = new StreamReader(req.Body, Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                req.Body.Position = 0;

                realBodyHash = Convert.ToHexString(
                    SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLower();
            }

            if (realBodyHash != reqBodyHash)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Body hash mismatch");
                return;
            }

            bool valid = _validator.Validate(
                req.Method, req.Path, ts, reqBodyHash, sig);

            if (!valid)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid HMAC");
                return;
            }

            await _next(context);
        }
    }
}
