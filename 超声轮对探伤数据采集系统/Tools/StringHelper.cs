using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class SubstringExtensions
    {
        /// <summary>
        /// 类似python字符串的操作
        /// </summary>
        /// <param name="text"></param>
        /// <param name="startIndex">起始字符串位置,包含startIndex</param>
        /// <param name="endIndex">结束字符串位置</param>
        /// <returns></returns>
        public static string ToSubString(this string text, int startIndex, int endIndex = 0)
        {
            if (text.Length == 0) throw new ArgumentNullException(nameof(text));
            if (startIndex < 0)
            {
                startIndex = text.Length + startIndex;
            }
            if (endIndex <= 0)
            {
                endIndex = text.Length + endIndex - 1;
            }

            if (startIndex >= text.Length || endIndex >= text.Length || startIndex < 0 || endIndex <= 0 || endIndex - startIndex < 0)
            {
                throw new IndexOutOfRangeException("StringHelper类SubstringExtensions:数组越界");
            }
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }
        /// <summary>
        /// 获取最后一个value之前的字符串，如果字符串不包含value，返回原字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SubstringBeforeLast(this string text, string value)
        {
            if (text.Length == 0) throw new ArgumentNullException(nameof(text));

            int index = text.LastIndexOf(value);
            if (index == -1)
            {
                return text;
            }
            int startIndex = text.LastIndexOf(value) + value.Length;
            if (startIndex >= text.Length)
            {
                return string.Empty;
            }
            else
            {
                return text.Substring(startIndex);
            }

        }
        /// <summary>
        /// 返回最后一个value后的字符串,如果没有返回原字符串
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SubstringAfterFirst(this string text, string value)
        {
            if (text.Length == 0) throw new ArgumentNullException(nameof(text));

            int endIndex = text.IndexOf(value);
            if (endIndex == -1)
            {
                return text;
            }
            if (endIndex + value.Length >= text.Length)
            {
                return string.Empty;
            }
            else
            {
                return text.Substring(endIndex + value.Length);
            }

        }
    }
}
