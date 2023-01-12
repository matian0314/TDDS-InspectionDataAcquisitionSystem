using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eth
{
    /// <summary>
    /// 通用控制
    /// </summary>
    public struct gen_cfg_t
    {       
        /// <summary>
        /// 0/停止工作 1/开始工作
        /// </summary>
        public int measure_en { get; set; }
        /// <summary>
        /// 触发频率, 如1000为所有通道1Khz工作
        /// </summary>
        public int trig_freq_hz { get; set; }      
        /// <summary>
        /// 0/外触发 1/自触发 2/编码器触发 触发控制
        /// </summary>
        public int out_trig_ctrl { get; set; }
        /// <summary>
        /// 0: 16ch-->1ch  1: 1ch-->16ch   
        /// </summary>
        public int out_trig_direct { get; set; }
        /// <summary>
        /// 外触发次数
        /// </summary>
        public int out_trig_num { get; set; }
        /// <summary>
        /// 输入接口滤波,默认为1
        /// </summary>
        public int input_filter_cfg { get; set; }
        /// <summary>
        /// 编码器脉冲触发间隔： 例如 1->2个脉冲触发一次 5->6个脉冲触发一次
        /// </summary>
        public int opt_trig_interval { get; set; }
        /// <summary>
        /// A波刷新频率（hz），10,20(调试用)
        /// </summary>
        public int wave_interval { get; set; }
        /// <summary>
        /// 声速，如钢中纵波 5900（调试用）
        /// </summary>
        public int wave_speed { get; set; }      
        /// <summary>
        /// 最大工作通道数
        /// </summary>
        public int max_chan { get; set; }
        /// <summary>
        /// 测厚下限
        /// </summary>
        public double thick_min { get; set; }
        /// <summary>
        /// 测厚上限
        /// </summary>
        public double thick_max { get; set; }
        /// <summary>
        /// 板卡类型
        /// </summary>
        public int board_cfg { get; set; }
        public int err_recover_interval { get; set; }
        /// <summary>
        /// 每帧A波包含字节数，（128,256,500）
        /// </summary>
        public int frame_total_len { get; set; }
        /// <summary>
        /// 输入io采集
        /// </summary>

        public int din_upload_en { get; set; }
        /// <summary>
        /// 输出报警宽度：ms
        /// </summary>
        public int output_alarm_width { get; set; }
        /// <summary>
        /// 输出喷枪宽度：ms
        /// </summary>
        public int output_gun_width { get; set; }   
        public static gen_cfg_t Create(int channelCount)
        {
            gen_cfg_t result = new gen_cfg_t();
            result.measure_en = 1;
            result.trig_freq_hz = 1000;
            result.out_trig_ctrl = 0;
            result.out_trig_direct = 1;
            //次数设定为30次看看效果
            result.out_trig_num = 30;
            result.input_filter_cfg = 1;
            //只和编码器相关，无效字段
            result.opt_trig_interval = 1;
            //只和自触发相关，无效字段
            result.wave_interval = 10;
            //和测厚有关，无效字段
            result.wave_speed = 5900;
            result.thick_min = 0;
            result.thick_max = 10;
            result.board_cfg = 0;
            result.err_recover_interval = 9;
            result.frame_total_len = 500;
            result.din_upload_en = 0;
            result.output_alarm_width = 0;
            result.output_gun_width = 0;
            result.max_chan = channelCount;

            return result;
        }
    }
}
