using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    public struct chan_cfg_t
    {       // 通道配置
        //直探头(Z)斜探头(X)斜探头-轮缘(Y)默认配置参数
        public static int XDefaultDb = 60;
        public static double XDefaultSoundPath = 400;
        public static double XWaveSpeed = 3230;
        public static int YDefaultDb = 70;
        public static double YDefaultSoundPath = 500;
        public static double YWaveSpeed = 3230;
        public static int ZDefaultDb = 55;
        public static double ZDefaultSoundPath = 60;
        public static double ZWaveSpeed = 5900;
        public int dest_chan { get; set; }
        public int write_chan { get; set; }
        /// <summary>
        /// 增益 0-100
        /// </summary>

        public int db { get; set; }
        /// <summary>
        /// 通道使能
        /// </summary>
        public int trig_en { get; set; }
        /// <summary>
        /// us 触发延时 (0,1,2,3,4...)
        /// </summary>
        public int trig_delay { get; set; }
        /// <summary>
        /// 0.01us 采样延时 (0,1,2,3,4...)
        /// </summary>
        public double sample_delay { get; set; }
        /// <summary>
        /// us 0-500
        /// </summary>
        public double sample_width { get; set; }
        /// <summary>
        /// 触发宽度=(n+1)*10 ns, trig_width=10-->110 ns
        /// </summary>
        public int trig_width { get; set; }
        /// <summary>
        /// 0--正半轴检波，1--负半轴检波，2--全检波
        /// </summary>
        public int half_sel { get; set; }
        /// <summary>
        /// 0/测厚 1/横前 2/横后 3/纵左 4/纵右 5/斜前 6/斜后 7/直探 8/分层
        /// </summary>
        public int func_type { get; set; }

        public int gate1_type { get; set; }     // 0/超出闸门报警 1/低于闸门报警
        public int gate1_enable { get; set; }
        public int gate2_type { get; set; }
        public int gate2_enable { get; set; }
        public int gate_track_enable { get; set; }
        public int gate4_type { get; set; }
        public int gate4_enable { get; set; }

        public double g1_start { get; set; }  // us
        public double g1_width { get; set; }  // us
        public double g2_start { get; set; }
        public double g2_width { get; set; }
        public double track_start { get; set; }
        public double track_width { get; set; }
        public double g4_start { get; set; }
        public double g4_width { get; set; }

        public int gate1_height { get; set; }   // 最高100，最低0	
        public int gate2_height { get; set; }
        public int gate4_height { get; set; }
        public int track_height { get; set; }

        public static chan_cfg_t Create(int channel, string Type)
        {
            chan_cfg_t result = new chan_cfg_t();
            result.dest_chan = channel;
            result.write_chan = channel;
            result.trig_en = 1;
            result.trig_delay = 0;
            result.half_sel = 2;

            result.func_type = 7;

            result.gate_track_enable = 0;
            result.track_start = 0;
            result.track_width = 10;
            result.track_height = 20;

            result.gate1_enable = 0;
            result.g1_start = 1;
            result.g1_width = 10;
            result.gate1_height = 10;
            result.gate1_type = 1;

            result.gate2_enable = 0;
            result.g2_start = 11;
            result.g2_width = 10;
            result.gate2_height = 20;
            result.gate2_type = 1;

            result.gate4_enable = 0;
            result.gate4_height = 10;
            result.gate4_type = 1;
            result.g4_start = 0;
            result.g4_width = 10;
            result.trig_width = 19;
            //500次采样的时间为sapmle_width us
            //声程 = 声速 * sapmle_width / 2
            //计算得 声程（mm） = 声速(m/s) * sample_width(us) / 2000
            switch (Type)
            {
                case "X":
                    result.db = XDefaultDb;
                    result.func_type = 5;
                    //250 实际声程约403.75mm
                    result.sample_width = 250;
                    //去掉前沿(15mm) 
                    result.sample_delay = 520;
                    break;
                case "Y":
                    result.db = YDefaultDb;
                    result.func_type = 5;
                    //实际声程约605.63mm
                    result.sample_width = 375;
                    //去掉前沿 （约8.5mm） 
                    result.sample_delay = 520;
                    break;
                case "Z":
                    result.db = ZDefaultDb;
                    result.func_type = 7;
                    //实际声程约92.19mm
                    result.sample_width = 31;
                    //去掉前沿 （约8.5mm）
                    result.sample_delay = 500;
                    break;
                default:
                    throw new Exception();
            }
            return result;
        }
        public static double GetPointInterval(string Type, chan_cfg_t config)
        {
            var realSamleWidth = Math.Ceiling(config.sample_width / 6.25) * 6.25;
            switch (Type)
            {
                case "斜探头-轮缘":
                    return realSamleWidth / 500 * XWaveSpeed / 2000;

                case "斜探头":
                    return realSamleWidth / 500 * YWaveSpeed / 2000;
                case "直探头":
                    
                    return realSamleWidth / 500 * ZWaveSpeed / 2000;
                default:
                    throw new Exception();
            }
        }
    }
}
