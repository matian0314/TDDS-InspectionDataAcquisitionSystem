using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Helpers
{
    public static class RegexHelper
    {
        /// <summary>
        /// 是否是v4.0版本的IP地址的字符串
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsValidIpV4(string ip)
        {
            string regex = @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$";
            ip = ip.Replace(" ", "");
            return Regex.IsMatch(ip, regex);
        }
        public static bool IsChinese(string chinese)
        {
            //将传入参数中的中文字符添加到结果字符串中
            for (int i = 0; i < chinese.Length; i++)
            {
                if (chinese[i] < 0x4E00 && chinese[i] > 0x9FA5) //汉字
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 保留中文字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string KeepChinese(string str)
        {
            //声明存储结果的字符串
            string chineseString = "";


            //将传入参数中的中文字符添加到结果字符串中
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] >= 0x4E00 && str[i] <= 0x9FA5) //汉字
                {
                    chineseString += str[i];
                }
            }


            //返回保留中文的处理结果
            return chineseString;
        }

        public static string KeepDigits(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var item in str)
            {
                if (item <= 9 && item >= 0)
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }

        public static string RemoveDigits(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in str)
            {
                if (item <= 9 && item >= 0)
                {
                    sb.Append(item);
                }
            }
            return sb.ToString();
        }
    }
}
