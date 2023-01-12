using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class DataTimeHelper
    {
        /// <summary>
        /// 例如时间为2022年06月7日 15:23:05秒
        /// 返回15时23分05秒
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToLongTimeChinese(this DateTime time)
        {
            return time.ToString("HH时mm分ss秒");
        }
        /// <summary>
        /// 例如时间为2022年06月7日 15:23:05秒
        /// 返回15时23分
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToShortTimeChinese(this DateTime time)
        {
            return time.ToString("HH时mm分");
        }
        /// <summary>
        /// 例如时间为2022年06月7日 15:23:05秒
        /// 返回2022年06月07日
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToLongDateChinese(this DateTime time)
        {
            return time.ToString("yyyy年MM月dd日");
        }
        /// <summary>
        /// 例如时间为2022年06月7日 15:23:05秒
        /// 返回2022年6月7日
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static string ToShortDateChinese(this DateTime time)
        {
            return time.ToString("yy年M月d日");
        }
        /// <summary>
        /// 将代表日期的字符串转换为DateTime
        /// 默认的格式为yyyyMMddHHmmss
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetDateTimeFromString(string time, string pattern = "yyyyMMddHHmmss")
        {
            return DateTime.ParseExact(time, pattern, null);
        }
    }
}
