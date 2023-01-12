using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    public class DefectMsg
    {
        public static int CurrentDefectIndex;
        public DefectSum Sum { get; set; }
        public List<DefectDetail> Data { get; set; } = new List<DefectDetail>();
    }
}
