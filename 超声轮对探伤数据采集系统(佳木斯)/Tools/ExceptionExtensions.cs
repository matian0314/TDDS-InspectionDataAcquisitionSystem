using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 将Exception对象序列化为Json格式
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="e"></param>
        /// <returns></returns>
        public static string ExceptionToJson<TException>(this TException e)
            where TException : Exception
        {
            return JsonConvert.SerializeObject(e);
        }
        /// <summary>
        /// 将Exception打印为字符串
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="ex"></param>
        /// <param name="options"></param>
        /// <returns></returns>

        public static string ToRecord<TException>(this TException ex, RecordOptions options = RecordOptions.Default)
            where TException : Exception
        {
            StringBuilder sb = new StringBuilder(1024);
            if (options.HasFlag(RecordOptions.Type))
            {
                sb.AppendLine($"Type: {ex.GetType().FullName}");
            }
            if (options.HasFlag(RecordOptions.TargetSite))
            {
                sb.AppendLine($"TargetSite: {ex.TargetSite}");
            }
            if (options.HasFlag(RecordOptions.Message))
            {
                sb.AppendLine($"Message: {ex.Message}");
            }
            if (options.HasFlag(RecordOptions.StackTrace))
            {
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
            }
            if (options.HasFlag(RecordOptions.Source))
            {
                sb.AppendLine($"Source: {ex.Source}");
            }
            if (options.HasFlag(RecordOptions.InnerException))
            {
                if (ex.InnerException != null)
                {
                    sb.AppendLine($"---------------InnerExceptions---------------");
                    sb.AppendLine(ex.InnerException.ToRecord(options));
                }
            }
            return sb.ToString();
        }
    }
    [Flags]
    public enum RecordOptions
    {
        Type = 1,
        TargetSite = 2,
        Message = 4,
        StackTrace = 8,
        Source = 16,
        InnerException = 32,
        Default = Message | StackTrace | TargetSite | Type | InnerException,
        All = Message | StackTrace | Source | TargetSite | Type | InnerException
    };
}
