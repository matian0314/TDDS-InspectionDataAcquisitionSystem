using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CardConfigurations;
using Eth;

namespace 超声轮对探伤数据采集系统
{
    public class CardInfoViewModel
    {
        public int Index { get; set; }
        public string CardName { get; set; }
        public string IP { get; set; }
        public string Status { get; set; }
        public LineName LineName { get; set; }
        public SolidColorBrush ForegroundColor { get; set; }
        public CardInfoViewModel(CardInfo card, List<string> ConnectedIpList)
        {
            this.Index = card.Index;
            this.CardName = card.CardName;
            this.IP = card.Ip;
            this.Status = ConnectedIpList.Contains(card.Ip) ? "已连接" : "未连接";
            this.LineName = card.LineName;
            if(this.Status == "已连接")
            {
                this.ForegroundColor = Brushes.Black;
            }
            else
            {
                this.ForegroundColor = Brushes.Red;
            }
        }
    }
}
