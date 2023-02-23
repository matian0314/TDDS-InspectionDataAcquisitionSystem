using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    public static class EthHelper
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(nameof(EthHelper));
        public static sbyte[] GetIpFromArray(this sbyte[,] connectIps, int index)
        {
            return new sbyte[]{ connectIps[4 * index / 100, 4 * index % 100], connectIps[4 * index / 100, 4 * index % 100 + 1] , connectIps[4 * index / 100, 4 * index % 100 + 2] ,connectIps[4 * index / 100, 4 * index % 100 + 3] };
        }
        public static string IpToString(sbyte[] ip)
        {
            if(ip.Length != 4)
            {
                throw new Exception("IP格式错误");
            }
            int[] result = { ip[0] > 0 ? ip[0] : 256 + ip[0], ip[1] > 0 ? ip[1] : 256 + ip[1], ip[2] > 0 ? ip[2] : 256 + ip[2], ip[3] > 0 ? ip[3] : 256 + ip[3] };
            return $"{result[0]}.{result[1]}.{result[2]}.{result[3]}";
        }
        public static sbyte[] ToIp(string Ip)
        {
            var ipString = Ip.Split('.');
            if(ipString.Length != 4)
            {
                log.Warn($"Ip地址为{Ip}，格式错误");
                return null;
            }
            sbyte[] result = new sbyte[4];
            for (int i = 0; i < 4; i++)
            {
                int value = Convert.ToInt32(ipString[i]);
                if(value <= 127 && value >= 0)
                {
                    result[i] = (sbyte)value;
                }
                else if(value <= 255 && value > 127)
                {
                    result[i] = (sbyte)(value - 256);
                }
                else
                {
                    log.Warn($"Ip地址为{Ip}，格式错误");
                    return null;
                }
            }
            return result;
        }
    }
}
