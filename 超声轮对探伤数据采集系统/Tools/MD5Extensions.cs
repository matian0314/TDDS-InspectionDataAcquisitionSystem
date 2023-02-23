using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace Tools
{
    /// <summary>
    /// 对字符串或流进行MD5算法。代码来自C#官方示例
    /// </summary>
    public static class MD5EncryptExtension
    {
        public static string MD5Encrpt(this string Message)
        {
            //或者使用 MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(Message));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data 
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
        }
    }
}
