namespace Insight.Utils.Server
{
    public class ServiceBase
    {
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
        public bool Verify(string key = null, string id = null)
        {
            var verify = new Verify();
            key = verify.userId == id ? null : key;
            if (!verify.Compare(key)) return false;

            var session = verify.result.data;
            tenantId = session.tenantId;
            deptId = session.deptId;
            userId = session.userId;
            userName = session.userName;

            return true;
        }
    }
}