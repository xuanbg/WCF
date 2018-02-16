using Insight.Utils.Entity;

namespace Insight.Utils.Server
{
    public class ServiceBase
    {
        protected CallManage callManage;
        protected string verifyUrl;

        public Result<object> result = new Result<object>();
        public string userName;
        public string userId;
        public string deptId;

        /// <summary>
        /// 身份验证方法
        /// </summary>
        /// <param name="action">操作权限代码，默认为空，即不进行鉴权</param>
        /// <param name="limit"></param>
        /// <returns>bool 身份是否通过验证</returns>
        public bool Verify(string action = null, int limit = 0)
        {
            var verify = new Verify(callManage, verifyUrl, action, limit);
            userName = verify.Token.userName;
            userId = verify.Token.userId;
            deptId = verify.Token.deptId;
            result = verify.result;

            return result.successful;
        }
    }
}