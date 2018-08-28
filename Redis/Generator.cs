using System;

namespace Insight.Utils.Redis
{
    public static class Generator
    {
        /// <summary>
        /// 根据指定的格式生成业务编码
        /// </summary>
        /// <param name="format">编码格式</param>
        /// <param name="group">分组格式</param>
        /// <returns>业务编码</returns>
        public static string newCode(string format, string group)
        {
            var index = format.IndexOf("#", StringComparison.Ordinal);
            if (index < 0) return null;

            format = replace(format);
            group = replace(group);

            var digits = format.Substring(index + 1, 1);
            var key = $"CodeGroup:{group}#{digits}";
            if (!LockHandler.getLock(group, 10)) return null;

            var number = 1;
            var val = RedisHelper.stringGet(key);
            if (!string.IsNullOrEmpty(val)) number = Convert.ToInt32(val) + 1;

            var len = Convert.ToInt32(digits);
            var code = toString(number, len);

            format = format.Replace($"#{len}", code);
            RedisHelper.stringSet(key, number);
            LockHandler.releaseLock(group);

            return format;
        }

        /// <summary>
        /// 日期格式替换为当前日期对应值
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>替换后字符串</returns>
        private static string replace(string str)
        {
            var array = new[] {"yyyy", "yy", "MM", "dd"};
            var now = DateTime.Now;
            foreach (var format in array)
            {
                var date = now.ToString(format);
                str = str.Replace(format, date);
            }

            return str;
        }

        /// <summary>
        /// 数字转制定位数的字符串，位数不足左补零，位数超出丢弃高位数字
        /// </summary>
        /// <param name="number">输入数字</param>
        /// <param name="length">字符串位数</param>
        /// <returns>左补零后的字符串</returns>
        private static string toString(int number, int length)
        {
            var code = number.ToString($"D{length}");
            var exceed = code.Length - length;
            if (exceed > 0) code = code.Substring(exceed, length);

            return code;
        }
    }
}
