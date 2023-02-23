using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤报文
{
    public class InspectionMsg
    {
        public DevStatus DevStatus { get; set; }
        public List<ProbeDetails> YW { get; set; } = new List<ProbeDetails>();
        public List<ProbeDetails> YN { get; set; } = new List<ProbeDetails>();
        public List<ProbeDetails> ZN { get; set; } = new List<ProbeDetails>();
        public List<ProbeDetails> ZW { get; set; } = new List<ProbeDetails>();
    }
}
