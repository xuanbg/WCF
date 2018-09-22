using System;
using System.Collections.Generic;

namespace Insight.Utils.Redis
{
    public static class Generator
    {
        private static Dictionary<string, string> garbleSet = RedisHelper.stringGet<Dictionary<string, string>>("GarbleSet");
        private static readonly Random random = new Random();

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

            var len = Convert.ToInt32(digits);
            if (len < 2 || len > 8) return null;

            var number = random.Next((int) Math.Pow(10, len));
            var val = RedisHelper.stringGet(key);
            if (!string.IsNullOrEmpty(val)) number = Convert.ToInt32(val) + 1;

            var code = toString(number, len);
            var i = len - 1;
            if (garbleSet == null) initSet();

            while (i > 0)
            {
                code = garble(code);
                i--;
            }

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

        /// <summary>
        /// 混淆字符串
        /// </summary>
        /// <param name="str">输入字符串</param>
        /// <returns>string 混淆后的字符串</returns>
        private static string garble(string str)
        {
            var len = str.Length;
            var frist = len > 2 ? str.Substring(0, 1) : "";
            var high = len > 2 ? str.Substring(1, len - 3) : "";
            var low = garbleSet[str.Substring(len - 2, 2)];

            return high + low + frist;
        }

        /// <summary>
        /// 初始化对照集
        /// </summary>
        private static void initSet()
        {
            var list = new List<string>();
            for (var i = 0; i < 100; i++)
            {
                list.Add(toString(i, 2));
            }

            garbleSet = new Dictionary<string, string>(100);
            for (var i = 0; i < 100; i++)
            {
                var index = random.Next(100 - i);
                garbleSet.Add(toString(i, 2), list[index]);
                list.RemoveAt(index);
            }

            RedisHelper.stringSet("GarbleSet", garbleSet);
        }
    }
}
