using System.Collections.Specialized;
using System.Net;
using System.ServiceModel.Web;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Utils.Server
{
    public class Verify
    {
        private readonly string baseServer = Util.GetAppSetting("BaseServer");
        private readonly string token;

        // 验证结果
        public Result<UserInfo> result = new Result<UserInfo>();

        // 用户ID
        public string userId;

        // 客户端IP地址
        public string ip;

        // 客户端信息
        public string userAgent;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Verify()
        {
            var context = WebOperationContext.Current;
            if (context == null) return;

            var request = context.IncomingRequest;
            var headers = request.Headers;
            ip = GetIp(headers);
            userAgent = headers.Get("user-agent");
            token = headers[HttpRequestHeader.Authorization];
            userId = Util.Base64ToAccessToken(token)?.userId;
        }

        /// <summary>
        /// 获取Http请求头部承载的Access Token
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>boll 是否通过验证</returns>
        public bool Compare(string key = null)
        {
            var url = $"{baseServer}/authapi/v1.0/tokens/verify?action={key}";
            var request = new HttpRequest(token);
            if (!request.Send(url))
            {
                result.BadRequest(request.message);
                return false;
            }

            result = Util.Deserialize<Result<UserInfo>>(request.data);

            return result.successful;
        }

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <param name="headers">请求头</param>
        /// <returns>string IP地址</returns>
        private static string GetIp(NameValueCollection headers)
        {
            var rip = headers.Get("X-Real-IP");
            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("X-Forwarded-For");
            }

            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("Proxy-Client-IP");
            }

            if (string.IsNullOrEmpty(rip))
            {
                rip = headers.Get("WL-Proxy-Client-IP");
            }

            return rip;
        }
    }
}