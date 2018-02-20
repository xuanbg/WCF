using System;

namespace Insight.Utils.Redis
{
    public class CallManage
    {
        /// <summary>
        /// 根据传入的时长返回当前调用的剩余限制时间（秒）
        /// </summary>
        /// <param name="key"></param>
        /// <param name="seconds">限制访问时长（秒）</param>
        /// <returns>int 剩余限制时间（秒）</returns>
        public int GetSurplus(string key, int seconds)
        {
            if (string.IsNullOrEmpty(key) || seconds == 0) return 0;

            var now = DateTime.Now;
            var val = now.ToString("O");
            var ts = new TimeSpan(0, 0, seconds);

            var limitKey = "Limit:" + key;
            var value = RedisHelper.StringGet(limitKey);
            if (string.IsNullOrEmpty(value))
            {
                RedisHelper.StringSet(limitKey, val, ts);
                return 0;
            }

            // 计算剩余时间，如剩余时间大于1秒，返回等待时间为剩余秒数
            var span = (now - DateTime.Parse(value)).TotalSeconds;
            var surplus = seconds - (int) span;
            if (surplus > 1) return surplus < 0 ? 0 : surplus;

            // 调用时间间隔低于1秒时,重置调用时间为当前时间作为惩罚
            RedisHelper.StringSet(limitKey, val, ts);
            return seconds;
        }

        public bool IsLimited(string key, int seconds, int max)
        {
            if (string.IsNullOrEmpty(key) || seconds == 0) return false;

            var ts = new TimeSpan(0, 0, seconds);
            var limitKey = "Limit:" + key;
            var value = RedisHelper.StringGet(limitKey);
            if (string.IsNullOrEmpty(value))
            {
                RedisHelper.StringSet(limitKey, "1", ts);
                return false;
            }

            // 读取访问次数,如次数超过限制,返回true,否则访问次数增加1次
            var count = Convert.ToInt32(value);
            var expire = RedisHelper.GetExpiry(limitKey);
            if (count > max) return true;

            count++;
            RedisHelper.StringSet(limitKey, count.ToString(), new TimeSpan(0, 0, expire));
            return false;
        }
    }
}