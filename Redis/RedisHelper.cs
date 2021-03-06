﻿using System;
using Insight.Utils.Common;
using StackExchange.Redis;

namespace Insight.Utils.Redis
{
    public static class RedisHelper
    {
        private static readonly string redisConn = Util.getAppSetting("Redis") ?? "localhost:6379";
        private static readonly string database = Util.getAppSetting("Database") ?? "6";
        private static readonly IDatabase redis = ConnectionMultiplexer.Connect(redisConn).GetDatabase(Convert.ToInt32(database));

        /// <summary>
        /// 是否存在指定的Key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>是否存在</returns>
        public static bool hasKey(string key)
        {
            return redis.KeyExists(key);
        }

        /// <summary>
        /// 删除指定的Key
        /// </summary>
        /// <param name="key">key</param>
        public static void delete(string key)
        {
            redis.KeyDelete(key);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="time">expiryTime</param>
        public static void stringSet(string key, object value, DateTime time)
        {
            stringSet(key, value, time - DateTime.Now);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="time">expiryTime</param>
        public static void stringSet(string key, string value, DateTime time)
        {
            stringSet(key, value, time - DateTime.Now);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="ts">TimeSpan</param>
        public static void stringSet(string key, object value, TimeSpan? ts = null)
        {
            stringSet(key, Util.serialize(value), ts);
        }

        /// <summary>
        /// 保存字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="ts">TimeSpan</param>
        public static void stringSet(string key, string value, TimeSpan? ts = null)
        {
            redis.StringSet(key, value, ts);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static string stringGet(string key)
        {
            return redis.StringGet(key);
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>value</returns>
        public static T stringGet<T>(string key)
        {
            var data = redis.StringGet(key);

            return Util.deserialize<T>(data);
        }

        /// <summary>
        /// 保存数据到哈希
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="field">field</param>
        /// <param name="value">value</param>
        public static void hashSet(string key, string field, string value)
        {
            redis.HashSet(key, field, value);
        }

        /// <summary>
        /// 读取哈希的指定字段
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="field">field</param>
        /// <returns>string value</returns>
        public static string hashGet(string key, string field)
        {
            return redis.HashGet(key, field);
        }

        /// <summary>
        /// 保存数据到集合
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        public static void setAdd(string key, string value)
        {
            redis.SetAdd(key, value);
        }

        /// <summary>
        /// 集合中是否包含指定的元素
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <returns>bool Has value</returns>
        public static bool setContains(string key, string value)
        {
            return redis.SetContains(key, value);
        }

        /// <summary>
        /// 获取指定key的剩余有效时间(秒)
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>剩余有效时间(秒)</returns>
        public static int getExpiry(string key)
        {
            var ts = redis.StringGetWithExpiry(key).Expiry;
            return (int) (ts?.TotalSeconds ?? 1);
        }
    }
}
