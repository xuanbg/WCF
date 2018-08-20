using Insight.Utils.Common;
using Insight.Utils.Entity;

namespace Insight.Utils.Server
{
    public class ServiceBase
    {
        public Result<object> result { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public string tenantId { get; private set; }

        /// <summary>
        /// 登录部门ID
        /// </summary>
        public string deptId { get; private set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string userId { get; private set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string userName { get; private set; }

        /// <summary>
        /// 会话合法性验证
        /// </summary>
        /// <param name="key">操作权限代码，默认为空，即不进行鉴权</param>
        /// <param name="id">用户ID</param>
        /// <returns>bool 身份是否通过验证</returns>
        public bool verify(string key = null, string id = null)
        {
            var verify = new Verify();
            key = verify.userId == id ? null : key;
            if (verify.compare(key))
            {
                var info = verify.result.data;
                tenantId = info.tenantId;
                deptId = info.deptId;
                userId = info.id;
                userName = info.name;
            }

            result = new Result<object>
            {
                successful = verify.result.successful,
                code = verify.result.code,
                name = verify.result.name,
                message = verify.result.message
            };

            return result.successful;
        }

        /// <summary>
        /// 获取客户端特征指纹
        /// </summary>
        /// <returns>string 客户端特征指纹</returns>
        public string getFingerprint()
        {
            var verify = new Verify();

            return Util.hash(verify.ip + verify.userAgent);
        }
    }
}