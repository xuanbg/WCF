using Insight.Utils.Entity;

namespace Insight.Utils.Server
{
    public class ServiceBase
    {
        protected string verifyUrl;

        private Result<object> result = new Result<object>();
        private string tokenId;
        private string tenantId;
        public string userName;
        private string userId;
        private string deptId;

        /// <summary>
        /// 身份验证方法
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <returns>bool 身份是否通过验证</returns>
        public bool Verify(string key = null)
        {
            var verify = new Verify(verifyUrl, key);
            userId = verify.Token.userId;
            result = verify.result;

            return result.successful;
        }
    }
}