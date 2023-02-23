using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tools
{
    public static class ReadExtensions
    {
        public static string ReadString(this BinaryReader br, int length)
        {
            return Encoding.Default.GetString(br.ReadBytes(length)).Replace("\0", "");
        }
    }
}
