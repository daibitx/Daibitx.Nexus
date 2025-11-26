using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Daibitx.Nuxus.Hmac
{
    public class HmacValidator
    {
        private readonly IOptionsMonitor<HmacOptions> _options;

        public HmacValidator(IOptionsMonitor<HmacOptions> options)
        {
            _options = options;
        }


        public bool Validate(string method, string path, string timestamp, string bodyHash, string signature)
        {
            var opts = _options.CurrentValue;
            if (string.IsNullOrEmpty(opts.SecretKey))
            {
                return false;
            }
            if (!long.TryParse(timestamp, out long ts))
            {
                return false;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (Math.Abs(now - ts) > opts.AllowedTimestampDriftSeconds)
            {
                return false;
            }
            string msg = $"{method}:{path}:{timestamp}:{bodyHash}";
            string expected = ComputeHmac(opts.SecretKey, msg);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));

        }
        public static string ComputeHmac(string secret, string message)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
