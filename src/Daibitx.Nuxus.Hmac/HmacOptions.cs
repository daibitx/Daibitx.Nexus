namespace Daibitx.Nuxus.Hmac
{
    public class HmacOptions
    {
        /// <summary>
        /// 双方共享的密钥，由网关和所有微服务共用。
        /// 通常来自环境变量 / K8S Secret / AgileConfig。
        /// </summary>
        public string SecretKey { get; set; } = "";

        /// <summary>
        /// 请求的最大允许时间偏差（秒）
        /// 默认 300 秒（5 分钟）防止重放攻击
        /// </summary>
        public int AllowedTimestampDriftSeconds { get; set; } = 300;
    }
}
