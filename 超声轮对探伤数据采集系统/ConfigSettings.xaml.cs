using Eth;
using MyLogger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using 超声轮对探伤数据采集系统.Eth;

namespace 超声轮对探伤数据采集系统
{
    /// <summary>
    /// ConfigSettings.xaml 的交互逻辑
    /// </summary>
    public partial class ConfigSettings : Window
    {

        public gen_cfg_t GeneralConfig { get; set; }
        public chan_cfg_t ChannelConfig { get; set; }
        public string Ip { get; set; }
        public string CardName { get; set; }
        public string ProbeName { get; set; }
        public string ProbeType { get; set; }
        public MainWindow MainWindow { get; set; }
        private static readonly SubscribeLogger log = SubscribeLogger.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public ConfigSettings(MainWindow MainWindow, gen_cfg_t genConfig, chan_cfg_t chnConfig)
        {
            GeneralConfig = genConfig;
            ChannelConfig = chnConfig;
            ProbeName = MainWindow.CurrentProbeInfo.ProbeName;
            ProbeType = MainWindow.CurrentProbeInfo.ProbeType;
            CardName = MainWindow.CurrentCardInfo.CardName;
            this.MainWindow = MainWindow;
            InitializeComponent();
            InitializeText();
        }

        private void InitializeText()
        {
            //GeneralConfig
            this.TrigControlComboBox.SelectedIndex = GeneralConfig.out_trig_ctrl;
            this.TrigDirectionComboBox.SelectedIndex = GeneralConfig.out_trig_direct;
            this.trig_freq_hz_textbox.Text = GeneralConfig.trig_freq_hz.ToString();
            this.out_trig_num_textbox.Text = GeneralConfig.out_trig_num.ToString();
            this.input_filter_cfg_textbox.Text = GeneralConfig.input_filter_cfg.ToString();
            this.wave_interval_textbox.Text = GeneralConfig.wave_interval.ToString();
            this.max_chan_textbox.Text = GeneralConfig.max_chan.ToString();
            this.wave_speed_textbox.Text = GeneralConfig.wave_speed.ToString();
            this.board_cfg_textbox.Text = GeneralConfig.board_cfg.ToString();
            this.frame_total_len_textbox.Text = GeneralConfig.frame_total_len.ToString();
            //ChannelConfig
            this.HalfSel.SelectedIndex = ChannelConfig.half_sel;
            this.FuncType.SelectedIndex = ChannelConfig.func_type;
            this.dest_chan_textblock.Text = ChannelConfig.dest_chan.ToString();
            this.db_textbox.Text = ChannelConfig.db.ToString();
            this.trig_delay_textbox.Text = ChannelConfig.trig_delay.ToString();
            this.sample_delay_textbox.Text = ChannelConfig.sample_delay.ToString();
            this.trig_width_textbox.Text = ChannelConfig.trig_width.ToString();
            this.sample_width_textbox.Text = ChannelConfig.sample_width.ToString();
            //Text
            this.CardNameTextBlock.Text = CardName;
            this.IpTextBlock.Text = Ip;
            this.ProbeNameTextblock.Text = ProbeName;
            this.ProbeTypeTextblock.Text = ProbeType;
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == ProbeName)?.SetChannelConfig(ChannelConfig);
            MainWindow.EthManager.Configs.Cards.FirstOrDefault(c => c.CardName == CardName)?.SetGeneralConfig(GeneralConfig);
            MainWindow.CurrentProbeInfo = MainWindow.EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == ProbeName);
            MessageBox.Show($"已设置板块{MainWindow.CurrentProbeInfo.LineName}探头的{MainWindow.CurrentProbeInfo.ProbeName}");
        }

        private void ConfigAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var probe in MainWindow.EthManager.Configs.Probes.Where(p => p.ProbeType == ProbeType))
            {
                var copy = probe.Config;
                copy.db = ChannelConfig.db;
                copy.sample_delay = ChannelConfig.sample_delay;
                copy.sample_width = ChannelConfig.sample_width;
                copy.trig_width = ChannelConfig.trig_width;
                copy.half_sel = ChannelConfig.half_sel;
                copy.func_type = ChannelConfig.func_type;
                probe.SetChannelConfig(copy);
            }
            MainWindow.CurrentProbeInfo = MainWindow.EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == ProbeName);
            MessageBox.Show($"已设置全部的{MainWindow.CurrentProbeInfo.ProbeType}");
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FuncType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ChannelConfig;
            item.func_type = FuncType.SelectedIndex;
            ChannelConfig = item;
        }

        private void HalfSel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = ChannelConfig;
            item.half_sel = HalfSel.SelectedIndex;
            ChannelConfig = item;
        }

        private void trig_freq_hz_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.trig_freq_hz = int.Parse(trig_freq_hz_textbox.Text);
            GeneralConfig = config;
        }

        private void TrigControlComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.out_trig_ctrl = TrigControlComboBox.SelectedIndex;
            GeneralConfig = config;
        }

        private void TrigDirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.out_trig_direct = TrigDirectionComboBox.SelectedIndex;
            GeneralConfig = config;
        }

        private void out_trig_num_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.out_trig_num = int.Parse(out_trig_num_textbox.Text);
            GeneralConfig = config;
        }

        private void input_filter_cfg_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.input_filter_cfg = int.Parse(input_filter_cfg_textbox.Text);
            GeneralConfig = config;
        }

        private void wave_interval_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.wave_interval = int.Parse(wave_interval_textbox.Text);
            GeneralConfig = config;
        }

        private void max_chan_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.max_chan = int.Parse(max_chan_textbox.Text);
            GeneralConfig = config;
        }

        private void wave_speed_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.wave_speed = int.Parse(wave_speed_textbox.Text);
            GeneralConfig = config;
        }

        private void board_cfg_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.board_cfg = int.Parse(board_cfg_textbox.Text);
            GeneralConfig = config;
        }

        private void frame_total_len_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = GeneralConfig;
            config.frame_total_len = int.Parse(frame_total_len_textbox.Text);
            GeneralConfig = config;
        }

        private void trig_delay_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = ChannelConfig;
            config.trig_delay = int.Parse(trig_delay_textbox.Text);
            ChannelConfig = config;
        }

        private void db_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = ChannelConfig;
            config.db = int.Parse(db_textbox.Text);
            ChannelConfig = config;
        }

        private void sample_delay_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = ChannelConfig;
            config.sample_delay = double.Parse(sample_delay_textbox.Text);
            ChannelConfig = config;
        }
        private void trig_width_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = ChannelConfig;
            config.trig_width = int.Parse(trig_width_textbox.Text);
            ChannelConfig = config;
        }
        private void sample_width_textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var config = ChannelConfig;
            config.sample_width = int.Parse(sample_width_textbox.Text);
            ChannelConfig = config;
        }
    }
}
