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
        public Result<Session> result = new Result<Session>();

        // 用户ID
        public string userId;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Verify()
        {
            var context = WebOperationContext.Current;
            if (context == null) return;

            var request = context.IncomingRequest;
            var headers = request.Headers;
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
                result.BadRequest(request.Message);
                return false;
            }

            result = Util.Deserialize<Result<Session>>(request.Data);

            return result.successful;
        }
    }
}