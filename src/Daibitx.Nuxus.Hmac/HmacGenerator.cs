using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Daibitx.Nuxus.Hmac
{
    public class HmacGenerator
    {
        private readonly IOptionsMonitor<HmacOptions> _options;

        public HmacGenerator(IOptionsMonitor<HmacOptions> options)
        {
            _options = options;
        }

        public (string Timestamp, string BodyHash, string Signature) Generate(
            string method,
            string path,
            string body)
        {
            var opts = _options.CurrentValue;

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string bodyHash = string.IsNullOrEmpty(body)
                ? ""
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLower();

            string msg = $"{method}:{path}:{timestamp}:{bodyHash}";
            string signature = HmacValidator.ComputeHmac(opts.SecretKey, msg);

            return (timestamp, bodyHash, signature);
        }
    }
}
