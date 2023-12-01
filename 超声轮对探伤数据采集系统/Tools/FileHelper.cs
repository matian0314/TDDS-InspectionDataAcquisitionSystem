using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class FileHelper
    {
        /// <summary>
        /// 异步，读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static async Task<string> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("未找到文件", path);
            }
            using (StreamReader sr = File.OpenText(path))
            {
                return await sr.ReadToEndAsync();
            }
        }
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("未找到文件", path);
            }
            using (StreamReader sr = File.OpenText(path))
            {
                return sr.ReadToEnd();
            }
        }
        /// <summary>
        /// 将content异步写入指定文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task WriteFileAsync(string path, string content)
        {
            using (StreamWriter sr = File.AppendText(path))
            {
                await sr.WriteLineAsync(content);
                await sr.FlushAsync();
            }
        }
        /// <summary>
        /// 将content写入指定文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void WriteFile(string path, string content)
        {
            using (StreamWriter sr = File.AppendText(path))
            {
                sr.WriteLine(content);
                sr.Flush();
            }
        }

        /// <summary>
        /// 将content写入指定文件，如果文件已经存在，则删除
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static void CreateFile(string path, string content)
        {
            if(File.Exists(path))
            { 
                File.Delete(path);
            }
            path = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            using (StreamWriter sr = File.AppendText(path))
            {
                sr.WriteLine(content);
                sr.Flush();
            }
        }
        public static void CreateFile(string path, byte[] content)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            path = Path.GetFullPath(path);
            var dir = Path.GetDirectoryName(path);
            Directory.CreateDirectory(dir);
            using (FileStream fs = File.Create(path))
            {
                fs.Write(content,0, content.Length);
                fs.Flush();
            }
        }
        public static bool FileOccupied(string path)
        {
            if (File.Exists(path)) return false;
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                return false;
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                fs?.Close();
            }
        }
        public static bool FileCanRead(string path)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fs?.Close();
            }
        }
        public static bool FileCanWrite(string path)
        {
            FileStream fs = null;
            try
            {
                fs = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.Write);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fs?.Close();
            }
        }
    }
}

