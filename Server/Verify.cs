using System;
using System.Net;
using System.ServiceModel.Web;
using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Utils.Server
{
    public class Verify
    {
        private AccessToken accessToken;
        private UriTemplateMatch uri;

        /// <summary>
        /// 验证结果
        /// </summary>
        public Result<object> result = new Result<object>();

        /// <summary>
        /// Access Token字符串
        /// </summary>
        public string token { get; private set; }

        /// <summary>
        /// Access Token对象
        /// </summary>
        public AccessToken Token
        {
            get
            {
                if (accessToken != null) return accessToken;

                accessToken = Util.Base64ToAccessToken(token);
                return accessToken;
            }
        }

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="verifyurl">验证服务URL</param>
        /// <param name="key">操作权限代码，默认为空(不进行鉴权)</param>
        public Verify(string verifyurl, string key = null)
        {
            if (!GetToken())
            {
                result.InvalidAuth();
                return;
            }

            var url = $"{verifyurl}?action={key}";
            var request = new HttpRequest(token);
            if (!request.Send(url))
            {
                result.BadRequest(request.Message);
                return;
            }

            result = Util.Deserialize<Result<object>>(request.Data);
        }

        /// <summary>
        /// 获取Http请求头部承载的Access Token
        /// </summary>
        /// <returns>boll Http请求头部是否承载Access Token</returns>
        private bool GetToken()
        {
            var context = WebOperationContext.Current;
            if (context == null) return false;

            var request = context.IncomingRequest;
            uri = request.UriTemplateMatch;

            var headers = request.Headers;
            token = headers[HttpRequestHeader.Authorization];
            if (!string.IsNullOrEmpty(token)) return true;

            var response = context.OutgoingResponse;
            response.StatusCode = HttpStatusCode.Unauthorized;
            return false;
        }
    }
}