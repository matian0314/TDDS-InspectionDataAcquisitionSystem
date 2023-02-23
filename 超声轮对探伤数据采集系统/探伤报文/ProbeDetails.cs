using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    public class ProbeDetails
    {
        public ProbStatus Status {get;set;}
        public List<AxleWave> Waves { get; set; } 
    }

    public class AxleWave
    {
        public List<byte> Wave1 { get; set; } = new List<byte>();
        public List<byte> Wave2 { get; set; } = new List<byte>();
        public List<byte> Wave3 { get; set; } = new List<byte>();
        public static AxleWave Read(BinaryReader br)
        {
            AxleWave result = new AxleWave();
            for (int i = 0; i < 500; i++)
            {
                result.Wave1.Add(br.ReadByte());
            }
            for (int i = 0; i < 500; i++)
            {
                result.Wave2.Add(br.ReadByte());
            }
            for (int i = 0; i < 500; i++)
            {
                result.Wave3.Add(br.ReadByte());
            }
            return result;
        }
    }
}
