using System;
using Insight.Utils.Common;
using StackExchange.Redis;

namespace Insight.Utils.Server
{
    public static class RedisHelper
    {
        private static readonly string redisConn = Util.GetAppSetting("Redis") ?? "localhost:6379";
        private static readonly string database = Util.GetAppSetting("Database") ?? "6";
        private static readonly IDatabase redis = ConnectionMultiplexer.Connect(redisConn).GetDatabase(Convert.ToInt32(database));

        /// <summary>
        /// 是否存在指定的Key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>是否存在</returns>
        public static bool HasKey(string key)
        {
            return redis.KeyExists(key);
        }

        /// <summary>
        /// 删除指定的Key
        /// </summary>
        /// <param name="key">key</param>
        public static void Delete(string key)
        {
            redis.KeyDelete(key);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void StringSet(string key, string value)
        {
            redis.StringSet(key, value);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="ts">TimeSpan</param>
        public static void StringSet(string key, string value, TimeSpan ts)
        {
            redis.StringSet(key, value, ts);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static string StringGet(string key)
        {
            return redis.StringGet(key);
        }

        /// <summary>
        /// 保存哈希
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="field">field</param>
        /// <param name="value">value</param>
        public static void HashSet(string key, string field, string value)
        {
            redis.HashSet(key, field, value);
        }

        /// <summary>
        /// 读取哈希
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="field">field</param>
        /// <returns></returns>
        public static string HashGet(string key, string field)
        {
            return redis.HashGet(key, field);
        }

        /// <summary>
        /// 获取指定key的剩余有效时间(秒)
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>剩余有效时间(秒)</returns>
        public static int GetExpiry(string key)
        {
            var ts = redis.StringGetWithExpiry(key).Expiry;
            return ts?.Seconds ?? 0;
        }
    }
}
