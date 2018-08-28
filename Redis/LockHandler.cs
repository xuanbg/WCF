using System;
using System.Threading;

namespace Insight.Utils.Redis
{
    public static class LockHandler
    {
        /// <summary>
        /// 获取分布式锁
        /// </summary>
        /// <param name="name">锁名称</param>
        /// <param name="tryTime">重试时间(秒)，默认5秒</param>
        /// <param name="expire">超时时间(秒)，默认值30秒</param>
        /// <returns></returns>
        public static bool getLock(string name, int tryTime = 5, int expire = 30)
        {
            if (string.IsNullOrEmpty(name)) return false;

            var key = $"Lock:{name}";
            var outTime = DateTime.Now.AddSeconds(tryTime);
            while (true)
            {
                if (DateTime.Now > outTime) return false;

                if (RedisHelper.hasKey(key))
                {
                    Thread.Sleep(100);
                    continue;
                }

                RedisHelper.stringSet(key, key, new TimeSpan(0, 0, 0, expire));
                return true;
            }
        }

        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="name">锁名称</param>
        public static void releaseLock(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            var key = $"Lock:{name}";
            RedisHelper.delete(key);
        }
    }
}
