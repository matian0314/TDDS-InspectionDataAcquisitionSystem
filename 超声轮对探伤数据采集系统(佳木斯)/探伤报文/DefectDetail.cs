using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    public class DefectDetail
    {
        public DefectInfo Info { get; set; }
        public DefectWave DefectWaves { get; set; } = new DefectWave();
    }

    public class DefectWave
    {
        public List<InitialWave> Waves { get; set; } = new List<InitialWave>();
    }

    public class InitialWave
    {
        public List<int> Wave { get; set; } = new List<int>();
    }
}
