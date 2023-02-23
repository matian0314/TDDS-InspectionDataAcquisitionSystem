using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Tools
{
    /// <summary>
    /// 使用序列化器完成的
    /// </summary>
    public static class XmlHelper
    {

        /// <summary>
        /// 通过XmlSerializer序列化实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static string ToXml<T>(this T t) where T : new()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(t.GetType());
            using (Stream stream = new MemoryStream())
            {
                xmlSerializer.Serialize(stream, t);
                stream.Position = 0;
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 字符串序列化成XML
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="content"></param>
        /// <returns></returns>
        public static T XmlToObject<T>(this string content) where T : new()
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(T));
                return (T)xmlFormat.Deserialize(stream);
            }
        }

        /// <summary>
        /// 将文件反序列化成实体类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public static T FileToObject<T>(string path) where T : new()
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("未找到文件", path);
            }

            using (Stream fStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                XmlSerializer xmlFormat = new XmlSerializer(typeof(T));
                return (T)xmlFormat.Deserialize(fStream);
            }
        }
    }
}
