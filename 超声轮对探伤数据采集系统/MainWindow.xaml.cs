using _485通信._485协议;
using Eth;
using Helpers;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using 报文转发.RabbitMQClient.Common.Configuration;
using CardConfigurations;
using 超声轮对探伤数据采集系统.Eth;
using Arction.Wpf.Charting;
using Arction.Wpf.Charting.Axes;
using Arction.Wpf.Charting.SeriesXY;
using System.Windows.Input;
using Arction.Wpf.Charting.Views.ViewXY;
using Arction.Wpf.Charting.Annotations;
using System.ComponentModel;
using System.Windows.Threading;
using 探伤报文;
using System.Reflection;

namespace 超声轮对探伤数据采集系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool InitializeComplete = false;
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 所有连接的板卡的IP地址
        /// </summary>
        public List<string> ConnectedControlBoxIpList { get; set; }
        public EthControlManager EthManager { get; set; }
        public bool IsRightSide { get; set; }

        /// <summary>
        /// 所有板卡（控制箱）的gen_cfg_t配置信息
        /// key为IP地址
        /// value为对应的板卡控制信息
        /// </summary>
        public Dictionary<string, gen_cfg_t> GeneralConfigs = new Dictionary<string, gen_cfg_t>();
        public ProbeInfo CurrentProbeInfo
        {
            get
            {
                if (_currentProbeInfo == null)
                {
                    if (IsRightSide)
                    {
                        _currentProbeInfo = EthManager.Configs.Probes.Where(p => p.LineName == LineName.YN || p.LineName == LineName.YW).First();
                        UpdateCurrentPrboeInfo(_currentProbeInfo);
                        UpdateCurrentCardInfo(_currentProbeInfo);
                    }
                    else
                    {
                        _currentProbeInfo = EthManager.Configs.Probes.Where(p => p.LineName == LineName.ZN || p.LineName == LineName.ZW).First();
                        UpdateCurrentPrboeInfo(_currentProbeInfo);
                        UpdateCurrentCardInfo(_currentProbeInfo);
                    }
                }
                return _currentProbeInfo;
            }
            set
            {
                _currentProbeInfo = value;
                UpdateCurrentPrboeInfo(value);
                UpdateCurrentCardInfo(value);
                var index = EthManager.Configs.Probes.FindIndex(p => p.ChannelName == _currentProbeInfo.ChannelName);
                if(index != -1)
                {
                    EthManager.Configs.Probes[index] = value;
                }
            }
        }
        private ProbeInfo _currentProbeInfo;
        public CardInfo CurrentCardInfo { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();
            CreateReviewChart();
            CreateLiveChart();
            Closing += MainWindow_Closing;
            InitializeComplete = true;
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            EthManager.OnClosing();
        }
        #region 初始化设置
        public void InitializeEvents()
        {
            log.Info("InitializeEvents");
            try
            {
                //CardConfigs.CreateTestConfigFileForJiamusi();
            
                EthManager = new EthControlManager();
                ConfigLeftOrRight();
                EthManager.DataGenerated += OnDataGenerated;
                EthManager.ConnectedIpListChanged += OnConnectedIpListChanged;
                DataRepo.RepoWrittenComplete += OnUpdateCollectionStatus;
                DataRepo.RepoWrittenComplete += InspectionInfo.CreateInspectionFileWithPythonScript;
                UpdateConnectionStatus(EthManager.ConnectedIpList);

                
                ConfigUILeftOrRight();

                RabbitMQClientConfigs.GetRabbitMQClientConfigs().Run();

                //LiveSeries = GetSeries();
                //ReviewSeries = GetSeries();
                //InitializeLabels();
                UpdateTimePeriodically();
                UpdateReviewUI();
                Task.Run(() =>
                {
                    string side = ConfigurationManager.AppSettings["Side"];
                    if (side == "左侧")
                    {
                        RabbitMQConsumer.StartReceiveMessage("485Left", OnReceive485Message);
                    }
                    else if (side == "右侧")
                    {
                        RabbitMQConsumer.StartReceiveMessage("485Right", OnReceive485Message);
                    }
                });
                IntializeFolders();
                //没有这句话，数据绑定不到这里来
                DataContext = this;
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
                throw;
            }

        }

        private void IntializeFolders()
        {
            Directory.CreateDirectory(ConfigurationManager.AppSettings["StoragePath"]);
            Directory.CreateDirectory(ConfigurationManager.AppSettings["SendFilePath"]);
            Directory.CreateDirectory(ConfigurationManager.AppSettings["InspectionMessageStoragePath"]);
            Directory.CreateDirectory(ConfigurationManager.AppSettings["SendDataPath"]);
            Directory.CreateDirectory(ConfigurationManager.AppSettings["CombineFilePath"]);
            Directory.CreateDirectory(ConfigurationManager.AppSettings["IntermediatePath"]);
            if(File.Exists(ConfigurationManager.AppSettings["PythonInterpreter"]) == false || File.Exists(ConfigurationManager.AppSettings["PythonScriptPath"]) == false)
            {
                log.Error("python脚本不存在!请检查");
                MessageBox.Show("python环境不完整!请检查");
            }
        }

        private void OnReceive485Message(string message)
        {
            var jmessage = (JObject)JsonConvert.DeserializeObject(message);
            if(jmessage["Command"].ToString() == ((int)0x5F).ToString())
            {
                var start = JsonConvert.DeserializeObject<StartUpSignal>(message);
                OnReceiveStartUpSignal(start);
            }
            else if(jmessage["Command"].ToString() == ((int)0x6F).ToString())
            {
                var end = JsonConvert.DeserializeObject<ShutDownSignal>(message);
                OnReceiveShutDownSignal(end);
            }
        }
        private void InitializeLabels()
        {
            //配置实时波形配置
            //默认用直探头和直探头的标准
            //double livePointInterval = UltrasonicTestingStandardHelper.GetPointInterval("直探头", 4);
            //LiveLabels = Enumerable.Range(1, 500).Select(l => (l * livePointInterval).ToString("0")).ToList();
            //double[] liveStandard = UltrasonicTestingStandardHelper.GetFlatBottomHole(0.4, livePointInterval, 10);
            //PutStandardToChart(LiveSeries, liveStandard);
        }

        private void UpdateTimePeriodically()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Start();
            timer.Tick += (obj, sender) =>
            {
                CurrentDate.Text = DateTime.Now.ToLongDateString();
                CurrentTime.Text = DateTime.Now.ToLongTimeString();
            };
        }


        private SeriesCollection GetSeries()
        {
            //实例化一条折线图
            LineSeries ch = new LineSeries()
            {
                //设置折线的标题
                Title = $"波形图",
                //折线图直线形式
                LineSmoothness = 1,
                //添加折线图的数据
                PointGeometry = null,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                PointGeometrySize = 1
            };
            LineSeries standard = new LineSeries()
            {
                LineSmoothness = 1,
                //添加折线图的数据
                PointGeometry = null,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                PointGeometrySize = 1,
                StrokeDashArray = new DoubleCollection() { 5 }
            };
            int[] initValue = new int[500];
            ch.Values = new ChartValues<int>(initValue);
            return new SeriesCollection() { ch, standard };
        }
        /// <summary>
        /// 读取软件主机是左侧还是右侧
        /// 配置IsRightSide属性
        /// </summary>
        private void ConfigLeftOrRight()
        {
            string side = ConfigurationManager.AppSettings["Side"];
            if (side == "左侧")
            {
                IsRightSide = false;
            }
            else if (side == "右侧")
            {
                IsRightSide = true;
            }
            else
            {
                throw new Exception("Side只能配置为\"左侧\"或\"右侧\"");
            }
        }
        /// <summary>
        /// 根据软件主机为左侧还是右侧，配置UI文字
        /// </summary>
        private void ConfigUILeftOrRight()
        {
            this.Title = $"超声轮对探伤数据采集系统 { (IsRightSide ? "右侧" : "左侧") } (机车专用) {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>().Version}";
            this.DataSide.Text = $"数据采集（{ (IsRightSide ? "右侧" : "左侧") }）";
            if (!IsRightSide)
            {
                this.NCardStatusTextBlock.Text = this.NCardStatusTextBlock.Text.Replace("Y", "Z");
                this.NProbeStatusTextBlock.Text = this.NProbeStatusTextBlock.Text.Replace("Y", "Z");
                this.WCardStatusTextBlock.Text = this.WCardStatusTextBlock.Text.Replace("Y", "Z");
                this.WProbeStatusTextBlock.Text = this.WProbeStatusTextBlock.Text.Replace("Y", "Z");
            }
        }
        private void UpdateConnectionStatus(List<string> ConnectedIpList)
        {
            log.Debug("UpdateConnectionStatus触发");
            if (IsRightSide)
            {
                var nCard = EthManager.Configs.Cards.Where(c => c.LineName == LineName.YN).Select(c => new CardInfoViewModel(c, ConnectedIpList)).OrderBy(c => c.Index).ToList();
                var wCard = EthManager.Configs.Cards.Where(c => c.LineName == LineName.YW).Select(c => new CardInfoViewModel(c, ConnectedIpList)).OrderBy(c => c.Index).ToList();
                var nProbe = EthManager.Configs.Probes.Where(c => c.LineName == LineName.YN).Select(p => new ProbeInfoViewModel(p, ConnectedIpList)).OrderBy(p => p.Index).ToList();
                var wProbe = EthManager.Configs.Probes.Where(c => c.LineName == LineName.YW).Select(p => new ProbeInfoViewModel(p, ConnectedIpList)).OrderBy(p => p.Index).ToList();
                var cardNameSource = EthManager.Configs.Cards.Where(c => c.LineName == LineName.YN || c.LineName == LineName.YW).Select(c => c.CardName).OrderBy(c => c).ToList();
                var reviewChannelSource = EthManager.Configs.Probes.Where(p => p.LineName == LineName.YN || p.LineName == LineName.YW).Select(p => p.Channel).Distinct().OrderBy(channel => channel).ToList();
                int[] axle = new int[32];
                for (int i = 0; i < 32; i++)
                {
                    axle[i] = i + 1;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InnerCardInfoGrid.ItemsSource = nCard;
                    OuterCardInfoGrid.ItemsSource = wCard;
                    InnerProbeInfoGrid.ItemsSource = nProbe;
                    OuterProbeInfoGrid.ItemsSource = wProbe;
                    ReviewCardName.ItemsSource = cardNameSource;
                    ReviewChannel.ItemsSource = reviewChannelSource;
                    ReviewAxle.ItemsSource = axle;
                    ReviewCardName.SelectedIndex = 0;
                    ReviewChannel.SelectedIndex = 0;
                    ReviewAxle.SelectedIndex = 0;
                });
            }
            else
            {
                var nCard = EthManager.Configs.Cards.Where(c => c.LineName == LineName.ZN).Select(c => new CardInfoViewModel(c, ConnectedIpList)).OrderBy(c => c.Index).ToList();
                var wCard = EthManager.Configs.Cards.Where(c => c.LineName == LineName.ZW).Select(c => new CardInfoViewModel(c, ConnectedIpList)).OrderBy(c => c.Index).ToList();
                var nProbe = EthManager.Configs.Probes.Where(c => c.LineName == LineName.ZN).Select(p => new ProbeInfoViewModel(p, ConnectedIpList)).OrderBy(p => p.Index).ToList();
                var wProbe = EthManager.Configs.Probes.Where(c => c.LineName == LineName.ZW).Select(p => new ProbeInfoViewModel(p, ConnectedIpList)).OrderBy(p => p.Index).ToList();
                var cardNameSource = EthManager.Configs.Cards.Where(c => c.LineName == LineName.ZN || c.LineName == LineName.ZW).Select(c => c.CardName).OrderBy(c => c).ToList();
                var reviewChannelSource = EthManager.Configs.Probes.Where(p => p.LineName == LineName.ZN || p.LineName == LineName.ZW).Select(p => p.Channel).Distinct().OrderBy(channel => channel).ToList();
                int[] axle = new int[32];
                for (int i = 0; i < 32; i++)
                {
                    axle[i] = i + 1;
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    InnerCardInfoGrid.ItemsSource = nCard;
                    OuterCardInfoGrid.ItemsSource = wCard;
                    InnerProbeInfoGrid.ItemsSource = nProbe;
                    OuterProbeInfoGrid.ItemsSource = wProbe;
                    ReviewCardName.ItemsSource = cardNameSource;
                    ReviewChannel.ItemsSource = reviewChannelSource;
                    ReviewAxle.ItemsSource = axle;
                    ReviewCardName.SelectedIndex = 0;
                    ReviewChannel.SelectedIndex = 0;
                    ReviewAxle.SelectedIndex = 0;
                });
            }
        }
        #endregion
        #region 事件处理函数
        private void OnReceiveShutDownSignal(object signal)
        {
            var shutdown = signal as ShutDownSignal;
            log.Debug($"收到关机信号,共{shutdown.AxleCnt}个轴");
            try
            {
                ShutDownEth();
            }
            catch (Exception ex)
            {
                log.Error("关机失败");
                log.Error(ex.ToRecord());
            }

        }
        private void OnReceiveStartUpSignal(object signal)
        {
            var startup = signal as StartUpSignal;
            log.Debug($"收到开机信号，速度为{startup.Speed / 1000.0 * 3.6}km/h");
            try
            {
                StartUpEth();
            }
            catch (Exception ex)
            {
                log.Error("开机失败");
                log.Error(ex.ToRecord());
            }
            
        }
        private void StartUpEth()
        {
            log.Debug("开启所有板卡和通道，来车灯亮");
            EthManager.EnableAllChannels();
            Dispatcher.Invoke(() => {
                EnableControlButton.IsEnabled = false;
                DisableControlButton.IsEnabled = true;
                VehicleComeLightOn();
            });

        }
        private void ShutDownEth()
        {
            log.Debug("关闭所有板卡和通道，系统灯亮");
            EthManager.DisableAllChannels();
            Dispatcher.Invoke(() =>
            {
                EnableControlButton.IsEnabled = true;
                DisableControlButton.IsEnabled = false;
                SystemLightOn();
            });

            DataRepo.WriteToFile(this.EthManager.Configs);
        }
        private void OnUpdateCollectionStatus(string _)
        {
            log.Debug("更新上次过车状态");
            UpdateCollectionStatus();
        }
        private void OnDataGenerated(int[] points, string ip, int channel)
        {
            if (CurrentProbeInfo == null) return;
            if (ip != CurrentProbeInfo.Ip) return;
            var writeChannel = CurrentProbeInfo.Channel - 1;
            if (channel != writeChannel)
            {
                return;
            }
            else
            {
                double interval = chan_cfg_t.GetPointInterval(CurrentProbeInfo.ProbeType, CurrentProbeInfo.Config);
                PutSamplesToChart(LiveChart, points, interval);
            }
        }
        private void OnConnectedIpListChanged(List<string> ConnectedIpList)
        {
            UpdateConnectionStatus(ConnectedIpList);
        }

        #endregion
        #region 按钮控制
        private void EnableControl_Button_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("EnableControl_Button_Click事件触发,开启所有板卡和通道，来车灯亮");
            EthManager.EnableAllChannelSelfTrigger();
            EnableControlButton.IsEnabled = false;
            DisableControlButton.IsEnabled = true;
            VehicleComeLightOn();

        }
        private void DisableControl_Button_Click(object sender, RoutedEventArgs e)
        {
            log.Debug("DisableControl_Button_Click事件触发,关闭所有板卡和通道，系统灯亮");
            EthManager.DisableAllChannelSelfTrigger();
            EnableControlButton.IsEnabled = true;
            DisableControlButton.IsEnabled = false;
            SystemLightOn();
            DataRepo.WriteToFile(this.EthManager.Configs);
        }
        private void InnerProbeTextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            var item = (DataGridRow)sender;
            TextBlock probeName = (TextBlock)InnerProbeInfoGrid.Columns[1].GetCellContent(item);
            if (probeName != null)
            {
                string name = probeName.Text;
                CurrentProbeInfo = EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == name);
            }
        }
        private void OuterProbeTextBox_GetFocus(object sender, RoutedEventArgs e)
        {
            var item = (DataGridRow)sender;
            TextBlock probeName = (TextBlock)OuterProbeInfoGrid.Columns[1].GetCellContent(item);
            if (probeName != null)
            {
                string name = probeName.Text;
                CurrentProbeInfo = EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == name);
            }
        }
        private void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var channel = int.Parse(ReviewChannel.Text);
            var ip = EthManager.Configs.Cards.First(c => c.CardName == ReviewCardName.Text).Ip;
            var axle = int.Parse(ReviewAxle.Text);
            DisplayReview(ip, channel, axle);
        }
        private void StopSamplingButton_Click(object sender, RoutedEventArgs e)
        {
            EthManager.DisableAllChannelSelfTrigger();
            StartSamplingButton.IsEnabled = true;
            StopSamplingButton.IsEnabled = false;
            DataRepo.WriteToFile(this.EthManager.Configs);
            SystemLightOn();
        }
        private void StartSamplingButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentProbeInfo == null || CurrentCardInfo == null) return;
            CurrentCardInfo.SetGeneralConfigSelfTrigger();
            foreach (var probe in EthManager.Configs.Probes.Where(p => p.Ip == CurrentCardInfo.Ip/* && p.ProbeName == CurrentProbeInfo.ProbeName*/))
            {
                probe.SetChannelSelfTrigger();
            }
            StartSamplingButton.IsEnabled = false;
            StopSamplingButton.IsEnabled = true;
            VehicleComeLightOn();
        }
        private void RestartConnection_Click(object sender, RoutedEventArgs e)
        {
            EthManager.RestartConnection();
        }
        private void ApplyConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigSettings window = new ConfigSettings(this, CurrentCardInfo.Config, CurrentProbeInfo.Config);
            window.ShowDialog();
        }

        private void ApplyConfigForAllButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigSettings window = new ConfigSettings(this, CurrentCardInfo.Config, CurrentProbeInfo.Config);
            window.ShowDialog();
        }
        #endregion
        #region 辅助函数
        private void VehicleComeLightOn()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //不声明为相对路径还找不到
                VehicleComeLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯亮.png", UriKind.Relative));
                ErrorLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
                SystemLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
            });

        }
        private void SystemLightOn()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                VehicleComeLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
                ErrorLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
                SystemLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯亮.png", UriKind.Relative));
            });
        }
        private void ErrorLightOn()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                VehicleComeLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
                ErrorLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯亮.png", UriKind.Relative));
                SystemLight.Source = new BitmapImage(new Uri("/Source/Icon/指示灯灭.png", UriKind.Relative));
            });
        }
        private void UpdateCurrentCardInfo(ProbeInfo value)
        {
            string ip = value.Ip;
            CurrentCardInfo = EthManager.Configs.Cards.Where(c => c.Ip == ip).First();
        }
        private void PutSamplesToChart(LightningChart chart, int[] points, double interval)
        {
            double[] value = points.Select(p => Math.Round(p / 2.55, 2)).ToArray();
            List<SeriesPoint> series = new List<SeriesPoint>();
            for (int i = 0; i < value.Length; i++)
            {
                series.Add(new SeriesPoint(i * interval, value[i]));
            }
            
            this.Dispatcher.Invoke(() =>
            {
                chart.BeginUpdate();
                var position = chart.ViewXY.LineSeriesCursors[0].ValueAtXAxis;
                var index = (int)(position / interval);
                if(index < 500)
                {
                    chart.ViewXY.Annotations[0].Text = $"声程: { position}\n幅值: { value[index]}% ";
                }
                chart.ViewXY.PointLineSeries[0].Points = series.ToArray();
                chart.ViewXY.XAxes[0].Minimum = 0;
                chart.ViewXY.XAxes[0].Maximum = interval * 500;
                
                chart.EndUpdate();
            });
        }
        private void UpdateCollectionStatus()
        {
            try
            {
                var nProbe = (List<ProbeInfoViewModel>)InnerProbeInfoGrid.ItemsSource;
                var wProbe = (List<ProbeInfoViewModel>)OuterProbeInfoGrid.ItemsSource;
                foreach (var item in nProbe)
                {
                    var channel = item.Channel - 1;
                    item.AxleCount = DataRepo.Repo.Where(e => e.Key.Ip == item.Ip && e.Key.Channel == channel).Count();
                    item.PointCount = DataRepo.Repo.Where(e => e.Key.Ip == item.Ip && e.Key.Channel == channel).Sum(e => e.Value.Count);
                }
                foreach (var item in wProbe)
                {
                    var channel = item.Channel - 1;
                    item.AxleCount = DataRepo.Repo.Where(e => e.Key.Ip == item.Ip && e.Key.Channel == channel).Count();
                    item.PointCount = DataRepo.Repo.Where(e => e.Key.Ip == item.Ip && e.Key.Channel == channel).Sum(e => e.Value.Count);
                }
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.InnerProbeInfoGrid.ItemsSource = null;
                    this.InnerProbeInfoGrid.ItemsSource = nProbe;
                    this.OuterProbeInfoGrid.ItemsSource = null;
                    this.OuterProbeInfoGrid.ItemsSource = wProbe;
                    this.LastVehicleTime.Text = DateTime.Now.ToString("F");
                });
            }
            catch (Exception ex)
            {
                log.Error(ex.ToRecord());
            }

            
        }
        public void UpdateCurrentPrboeInfo(ProbeInfo value)
        {
            CurrentProbeName.Text = value.ProbeName;
            CurrentProbeCard.Text = value.CardName;
            CurrentProbeSoundPath.Text = value.SoundPath.ToString("N2");
            CurrentProbeDb.Text = value.Db.ToString();
            CurrentSampleDelay.Text = value.Config.sample_delay.ToString();
            CurrentProbeSampleWidth.Text = value.Config.sample_width.ToString();
            UpdateChartLabelsAndStandard(LiveChart, value);
            var index = EthManager.Configs.Probes.FindIndex(p => p.ChannelName == _currentProbeInfo.ChannelName);
            if (index != -1)
            {
                EthManager.Configs.Probes[index] = value;
            }
        }
        public void RestartCardConfigs_Click(object sender, RoutedEventArgs e)
        {
            EthManager.ResetAllCardConfigs();
            CurrentProbeInfo = EthManager.Configs.Probes.FirstOrDefault(p => p.ProbeName == CurrentProbeInfo.ProbeName);
            MessageBox.Show("已将板卡设置恢复为默认");
        }
        private void UpdateChartLabelsAndStandard(LightningChart chart, ProbeInfo probe)
        {
            if (chart == null) return;
            double interval = chan_cfg_t.GetPointInterval(probe.ProbeType, probe.Config);
            double[] points;
            if(probe.ProbeType == "直探头")
            {
                points = UltrasonicTestingStandardHelper.GetFlatBottomHole(0.4, interval);
            }
            else
            {
                points = UltrasonicTestingStandardHelper.GetHorizontalHoleStandard(0.4, interval);
            }
            List<SeriesPoint> seriesPoints = new List<SeriesPoint>(points.Count());
            for (int i = 0; i < points.Count(); i++)
            {
                seriesPoints.Add(new SeriesPoint(interval * i, points[i]));
            }

            if (chart == ReviewChart)
            {
                Dispatcher.Invoke(() =>
                {
                    ReviewChart.BeginUpdate();
                    ReviewChart.ViewXY.XAxes[0].Maximum = interval * 500;
                    ReviewChart.ViewXY.PointLineSeries[1].Points = seriesPoints.ToArray();
                    ReviewChart.EndUpdate();
                    log.Info($"更新回看波形图的探伤参考线,探头为{probe.CardName}板卡的第{probe.Channel}个通道,IP为{probe.Ip}");
                });
            }
            else// if(chart == LiveChart)
            {
                Dispatcher.Invoke(() =>
                {
                    LiveChart.BeginUpdate();
                    LiveChart.ViewXY.XAxes[0].Maximum = interval * 500;
                    LiveChart.ViewXY.PointLineSeries[1].Points = seriesPoints.ToArray();
                    LiveChart.EndUpdate();
                    log.Info($"更新实时波形图的探伤参考线,探头为{probe.CardName}板卡的第{probe.Channel}个通道,IP为{probe.Ip}");
                });

                
            }
        }
        #endregion
        #region 测试用函数
        /// <summary>
        /// 仅测试用，周期打开所有通道，记录数据
        /// </summary>
        private void EnableChannelsPeriodically()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        EthManager.DisableAllChannels();
                        DataRepo.WriteToFile(this.EthManager.Configs);
                        Thread.Sleep(20 * 1000);
                        EthManager.EnableAllChannels();
                        Thread.Sleep(300 * 1000);
                    }
                    catch (Exception ex)
                    {

                        log.Error(ex.ToRecord());
                    }

                }
            });
        }
        private void TestVehicleCome()
        {     
            Random rand = new Random(Seed : DateTime.Now.Millisecond);
            Action<string> act = (ip) =>
            {
                //32个轴
                for (int axle = 1; axle <= 32; axle++)
                {
                    //一个板卡有16个通道
                    for (int channel = 0; channel < 16; channel++)
                    {
                        //每次采集触发31次信号
                        for (int i = 0; i < 31; i++)
                        {
                            byte[] data = new byte[500];
                            rand.NextBytes(data);
                            EthDev.TestEthHandler(ip, channel, data);
                        }
                        Thread.Sleep(TimeSpan.FromMilliseconds(100000));
                    }
                }
            };
            foreach (var card in this.EthManager.Configs.Cards)
            {
                Task.Run(() => act(card.Ip));
            }
            Thread.Sleep(TimeSpan.FromSeconds(150));
            DataRepo.WriteToFile(this.EthManager.Configs);
        }
        #endregion
        private string BaseDirPath = ConfigurationManager.AppSettings["StoragePath"];
        private string _selectedTime = "";
        public string SelectedTime
        {
            get => _selectedTime;
            set
            {
                _selectedTime = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedTime)));
            }
        }
        private void SelectDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog()
            {
                SelectedPath = Path.Combine(BaseDirPath, _selectedTime)
            };
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedTime = dialog.SelectedPath.Split('\\').Last();
            }
        }
        private void DisplayReview(string ip, int channel, int axle)
        {
            UpdateReviewUI();
            log.Info($"播放回放记录，IP为{ip},通道为{channel},轴号为{axle}");
            var probe = EthManager.Configs.Probes.First(p => p.Ip == ip && p.Channel == channel);
            UpdateChartLabelsAndStandard(ReviewChart, probe);
            var file = Directory.GetFiles(System.IO.Path.Combine(BaseDirPath, SelectedTime), $"{ip}-{axle}.dat").FirstOrDefault();
            if(file == null)
            {
                ShowMessageBox("文件不存在");
                return;
            }
            else
            {
                log.Info($"查看{file}文件");
                using(StreamReader sr = new StreamReader(file))
                {
                    var readIndex = channel;
                    while(readIndex > 1)
                    {
                        sr.ReadLine();
                        readIndex--;
                    }
                    var json = sr.ReadLine();
                    KeyValuePair<RepoKey, List<byte[]>> keyValuePair = JsonConvert.DeserializeObject<KeyValuePair<RepoKey, List<byte[]>>>(json);
                    int count = keyValuePair.Value.Count;
                    
                    Task.Factory.StartNew(() =>
                    {
                        double miliSeconds = double.Parse(ConfigurationManager.AppSettings["ReplayIntervalTime"]);
                        var interval = chan_cfg_t.GetPointInterval(probe.ProbeType, probe.Config);
                        for (int i = 0; i < count; i++)
                        {
                            PutSamplesToChart(this.ReviewChart, keyValuePair.Value[i].Select(v => (int)v).ToArray(), interval);
                            Thread.Sleep(TimeSpan.FromMilliseconds(miliSeconds));
                        }
                    });
                }
            }
        }
        private void ShowMessageBox(string alert)
        {
            var result = MessageBox.Show(alert);
            if(result == MessageBoxResult.OK)
            {
                return;
            }
        }

        private void Review_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(InitializeComplete)
            {
                //var combox = sender as ComboBox;
                //combox.SelectedIndex = 0;
                UpdateReviewUI();
            }
            
        }
        private void UpdateReviewUI()
        {
            //ComboBox控件的Text属性尚未更改，但SelectedItem等属性已经更改了
            //1111111
            var channel = int.Parse(ReviewChannel.SelectedItem?.ToString());
            var cardName = ReviewCardName.SelectedItem?.ToString();
            var axle = int.Parse(ReviewAxle.SelectedItem?.ToString());
            var probe = EthManager.Configs.Probes.FirstOrDefault(p => p.CardName == cardName && p.Channel == channel);
            if (probe == null) return;
            ReviewSoundPath.Text = probe?.SoundPath.ToString("0.00");
            ReviewDb.Text = probe?.Db.ToString("0.0");

            var pointInterval = chan_cfg_t.GetPointInterval(probe.ProbeType, probe.Config);
            double[] standard;
            if (probe.ProbeType == "直探头")
            {
                standard = UltrasonicTestingStandardHelper.GetFlatBottomHole(0.4, pointInterval);
            }
            else
            {
                standard = UltrasonicTestingStandardHelper.GetHorizontalHoleStandard(0.4, pointInterval);
            }
            UpdateChartLabelsAndStandard(ReviewChart, probe);
        }
        #region LightningChart
        private LightningChart ReviewChart;
        private LightningChart LiveChart;



        private void CreateReviewChart()
        {
            var channel = int.Parse(ReviewChannel.SelectedItem.ToString());
            var cardName = ReviewCardName.SelectedItem.ToString();
            var axle = int.Parse(ReviewAxle.SelectedItem.ToString());
            var probe = EthManager.Configs.Probes.First(p => p.CardName == cardName && p.Channel == channel);
            ReviewChart = CreateChart((chart) =>
            {
                chart.Title.Text = $"板卡{ReviewCardName.Text}-通道{ReviewChannel.Text}-第{ReviewAxle.Text}轴";
                chart.ViewXY.XAxes[0].Maximum = chan_cfg_t.GetPointInterval(probe.ProbeType, probe.Config) * 500;
                chart.ViewXY.YAxes[0].Maximum = 100;
                var grid = ReviewChartGrid.Children.Add(chart);
            });
            UpdateChartLabelsAndStandard(ReviewChart, probe);
        }
        private void CreateLiveChart()
        {
            LiveChart = CreateChart((chart) =>
            {
                chart.Title.Text = $"板卡{CurrentProbeCard.Text}-通道{CurrentProbeName.Text}";
                chart.ViewXY.XAxes[0].Maximum = chan_cfg_t.GetPointInterval(CurrentProbeInfo.ProbeType, CurrentProbeInfo.Config) * 500;
                chart.ViewXY.YAxes[0].Maximum = 100;
                var grid = LiveChartGrid.Children.Add(chart);
            });
            UpdateChartLabelsAndStandard(LiveChart, CurrentProbeInfo);
        }
        private LightningChart CreateChart(Action<LightningChart> chartConfig)
        {
            LightningChart _chart = new LightningChart();
            _chart.BeginUpdate();
            //Reduce memory usage and increase performance
            _chart.ViewXY.DropOldSeriesData = true;

            //Keep event markers
            _chart.ViewXY.DropOldEventMarkers = true;

            //背景颜色和背景渐变色
            _chart.ChartBackground.Color = Colors.Silver;
            _chart.ChartBackground.GradientColor = Colors.Gray;

            //图表 背景颜色和渐变色
            //Argb Alpha(透明度),Red,Green,Blue
            //Color.FromArgb(255, 0, 64, 64)为一种 深青色
            _chart.ViewXY.GraphBackground.Color = Colors.DarkGreen;//Color.FromArgb(255, 0, 64, 64);
            _chart.ViewXY.GraphBackground.GradientColor = Colors.ForestGreen;
            _chart.ViewXY.GraphBackground.GradientDirection = 90;
            _chart.ViewXY.GraphBackground.GradientFill = GradientFill.Linear;

            //禁止图表的XY轴自动调整尺寸
            _chart.ViewXY.AxisLayout.AutoAdjustMargins = false;
            _chart.ViewXY.AxisLayout.XAxisAutoPlacement = XAxisAutoPlacement.Off;
            _chart.ViewXY.AxisLayout.YAxisAutoPlacement = YAxisAutoPlacement.Off;
            _chart.ViewXY.AxisLayout.XAxisTitleAutoPlacement = false;
            _chart.ViewXY.AxisLayout.YAxisTitleAutoPlacement = false;

            //设置边缘厚度，Margin为0时，ChartBackground颜色被完全覆盖
            _chart.ViewXY.Margins = new Thickness(30, 30, 20, 0);

            //移除已经存在的Y轴
            _chart.ViewXY.YAxes.Clear();

            //禁止利用鼠标放大缩小图表
            _chart.ViewXY.ZoomPanOptions.DevicePrimaryButtonAction = UserInteractiveDeviceButtonAction.None;
            _chart.ViewXY.ZoomPanOptions.DeviceSecondaryButtonAction = UserInteractiveDeviceButtonAction.None;
            //_chart.ViewXY.ZoomPanOptions.ZoomRectLine.Color = Colors.White;
            //_chart.ViewXY.ZoomPanOptions.ZoomRectLine.Width = 3;

            //设置标题
            _chart.Title.Color = Colors.Orange;
            _chart.Title.Shadow.Style = TextShadowStyle.Off;
            _chart.Title.Offset.SetValues(0, 20);

            //XY轴的Grid颜色
            Color colorMajorGrid = Colors.Yellow;//Color.FromArgb(180, 255, 165, 0);//橘色
            Color color = ChartTools.CalcGradient(colorMajorGrid, Colors.White, 50);
            color.A = 200;

            #region 配置X轴
            _chart.ViewXY.XAxes.Clear();

            AxisX axisX = new AxisX(_chart.ViewXY);
            //X轴不进行滚动，而是固定不动的，以下配置项均不使用
            {
                axisX.ScrollMode = XAxisScrollMode.None;
                axisX.SweepingGap = 0;
                axisX.SteppingInterval = 0.001;
            }
            //X轴范围
            axisX.Visible = true;
            axisX.Minimum = 0;
            axisX.AllowScaling = false;
            axisX.AllowScrolling = false;
            axisX.AxisThickness = 0;
            //分隔符 
            axisX.MajorDivTickStyle.Visible = true;
            axisX.MinorDivTickStyle.Visible = true;
            axisX.MajorGrid.Color = colorMajorGrid;
            axisX.MajorGrid.Visible = true;
            axisX.MinorGrid.Visible = false;
            axisX.AutoDivSpacing = false;
            axisX.MajorDivCount = 10;//和AutoDivSpacing有一个起效
            axisX.KeepDivCountOnRangeChange = true;
            //标题
            axisX.Title.Visible = true;
            axisX.Title.Text = "声程(mm)";
            axisX.Title.HorizontalAlign = XAxisTitleAlignmentHorizontal.Right;
            axisX.Title.VerticalAlign = XAxisTitleAlignmentVertical.Bottom;
            axisX.LabelsVisible = true;
            axisX.LabelsPosition = Alignment.Near;
            _chart.ViewXY.XAxes.Add(axisX);
            #endregion
            #region 配置Y轴
            AxisY axisY = new AxisY(_chart.ViewXY)
            {
                AllowAutoYFit = true,
                AutoDivSpacing = false,
                MajorDivCount = 5
            };
            axisY.Title.Visible = true;
            axisY.Title.Angle = 0;
            axisY.Title.HorizontalAlign = YAxisTitleAlignmentHorizontal.Right;
            axisY.Title.VerticalAlign = YAxisTitleAlignmentVertical.Top;
            axisY.Title.Text = "幅值";
            axisY.Alignment = AlignmentHorizontal.Right;
            axisY.AxisThickness = 3;

            axisY.LabelsVisible = true;
            axisY.LabelsColor = color;
            axisY.LabelsAngle = 0;
            axisY.AutoFormatLabels = true;
            axisY.LabelTicksGap = 5;


            axisY.MajorGrid.Color = colorMajorGrid;
            axisY.MajorDivTickStyle.Visible = true;
            axisY.MinorDivTickStyle.Visible = true;
            axisY.MinorGrid.Color = Color.FromArgb(40, 255, 255, 255);
            axisY.MinorGrid.Visible = true;


            axisY.AllowScaling = false;
            axisY.AllowScrolling = false;
            _chart.ViewXY.YAxes.Add(axisY);
            //多个Y轴时的处理方式
            _chart.ViewXY.AxisLayout.YAxesLayout = YAxesLayout.Layered;
            #endregion
            //通道数据
            //annotation 注释
            //area series 区域
            //bar series 柱状图
            //polygen series 多边形
            PointLineSeries line = new PointLineSeries(_chart.ViewXY, axisX, axisY);

            line.Title.HorizontalAlign = AlignmentHorizontal.Left;
            line.Title.AllowUserInteraction = false;
            line.Title.Offset.SetValues(0, -10);
            line.Title.Visible = false;
            line.LineStyle.Color = Colors.White;
            line.PointStyle.Color1 = Colors.White;
            line.Title.Color = line.LineStyle.Color;
            line.LineStyle.Width = 1f;
            line.LineStyle.AntiAliasing = LineAntialias.None;
            line.AllowUserInteraction = false;
            _chart.ViewXY.PointLineSeries.Add(line);

            PointLineSeries standardLine = new PointLineSeries(_chart.ViewXY, axisX, axisY);

            standardLine.Title.HorizontalAlign = AlignmentHorizontal.Left;
            standardLine.Title.AllowUserInteraction = false;
            standardLine.Title.Offset.SetValues(0, -10);
            standardLine.Title.Visible = false;
            standardLine.LineStyle.Color = Colors.Orange;
            standardLine.PointStyle.Color1 = Colors.Orange;
            standardLine.Title.Color = line.LineStyle.Color;
            standardLine.LineStyle.Width = 1f;
            standardLine.LineStyle.AntiAliasing = LineAntialias.None;
            standardLine.AllowUserInteraction = false;
            _chart.ViewXY.PointLineSeries.Add(standardLine);
            //Cursor
            LineSeriesCursor cursor = new LineSeriesCursor(_chart.ViewXY, axisX);
            cursor.PositionChanged += cursor_PositionChanged;
            cursor.ValueAtXAxis = 5;
            cursor.LineStyle.Color = Colors.Orange;
            cursor.SnapToPoints = true;
            cursor.TrackPoint.Color1 = Colors.White;
            cursor.LineStyle.Width = 2f;
            cursor.HighlightedStateChanged += Cursor_HighlightedStateChanged;
            _chart.ViewXY.LineSeriesCursors.Add(cursor);
            //Add an annotation to show the cursor values
            AnnotationXY cursorValueDisplay = new AnnotationXY(_chart.ViewXY, _chart.ViewXY.XAxes[0], _chart.ViewXY.YAxes[0])
            {
                Style = AnnotationStyle.RoundedRectangle,
                LocationCoordinateSystem = CoordinateSystem.AxisValues
            };
            //cursorValueDisplay.LocationRelativeOffset.X = 0;
            //cursorValueDisplay.LocationRelativeOffset.Y = 0;
            cursorValueDisplay.Sizing = AnnotationXYSizing.Automatic;
            cursorValueDisplay.TextStyle.Font = new WpfFont("楷体", 8f, false, false);
            cursorValueDisplay.TextStyle.Color = Colors.Yellow;
            cursorValueDisplay.Text = "";
            cursorValueDisplay.AllowTargetMove = false;
            cursorValueDisplay.Fill.Color = Colors.Transparent;
            cursorValueDisplay.Fill.GradientColor = Colors.Transparent;
            cursorValueDisplay.BorderVisible = false;
            cursorValueDisplay.Visible = false;
            _chart.ViewXY.Annotations.Add(cursorValueDisplay);

            //Don't show legend box
            _chart.ViewXY.LegendBoxes[0].Visible = false;

            //Disable hardware anti-aliasing
            _chart.ChartRenderOptions.AntiAliasLevel = 0;

            //Set enhanced anti-aliasing. It makes GPU side line widening and gaussian blur. 
            _chart.ChartRenderOptions.ViewXY.LineSeriesEnhancedAntiAliasing = EnhancedAntiAliasing.On;

            chartConfig?.Invoke(_chart);

            //Allow chart rendering
            _chart.EndUpdate();
            return _chart;
        }

        private void Cursor_HighlightedStateChanged(object sender, HighlightedStateEventArgs e)
        {
            var cursor = sender as LineSeriesCursor;
            var annotation = cursor.OwnerView.Annotations[0];
            var chart = cursor.OwnerView.OwnerChart;
            chart.Dispatcher.Invoke(() =>
            {
                chart.BeginUpdate();
                if (e.IsHighlighted)
                {
                    annotation.Visible = true;
                }
                else
                {
                    if(!annotation.IsHighlighted())
                    {
                        annotation.Visible = false;
                    }
                    
                }
                chart.EndUpdate();
            });

        }
        private void cursor_PositionChanged(object sender, PositionChangedEventArgs e)
        {
            e.CancelRendering = true;
            var cursor = sender as LineSeriesCursor;
            var position = e.NewValue;

            var _chart = cursor.OwnerView.OwnerChart;
            var series = _chart.ViewXY.PointLineSeries[0];
            LineSeriesValueSolveResult result = series.SolveYValueAtXValue(position);
            if(result.SolveStatus == LineSeriesSolveStatus.OK)
            {
                var y = (result.YMax + result.YMin) / 2.0;
                var annotation = _chart.ViewXY.Annotations[0];
                annotation.LocationAxisValues.X = position + 5;
                annotation.LocationAxisValues.Y = y;
                annotation.Text = $"声程:{position:N2}\t幅值:{y:N2}%";
            }
            else
            {
                return;
            }


        }
        #endregion

        private void CurrentProbeSampleDelay_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            var textbox = sender as TextBox;
            int sampleWidth;
            try
            {
                sampleWidth = int.Parse(textbox.Text);
                if (sampleWidth < 0 || sampleWidth > 500) throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show($"无效的采样宽度{textbox.Text}");
                textbox.Text = CurrentProbeInfo.Config.sample_width.ToString();
                return;
            }
            CurrentProbeInfo.Config.sample_width = sampleWidth;
            CurrentProbeInfo.SetChannelConfig(CurrentProbeInfo.Config);
            CurrentProbeSampleWidth.Text = CurrentProbeInfo.Config.sample_width.ToString();
            UpdateCurrentPrboeInfo(CurrentProbeInfo);
        }

        private void CurrentSampleDelay_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            int sampleDelay = 0;
            try
            {
                sampleDelay = int.Parse(CurrentSampleDelay.Text);
                if (sampleDelay < 0) throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show($"无效的采样延迟{CurrentSampleDelay.Text}");
                CurrentSampleDelay.Text = CurrentProbeInfo.Config.sample_delay.ToString();
                return;
            }
            CurrentProbeInfo.Config.sample_delay = sampleDelay;
            CurrentProbeInfo.SetChannelConfig(CurrentProbeInfo.Config);
            UpdateCurrentPrboeInfo(CurrentProbeInfo);
        }

        private void CurrentProbeDb_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            int db = 0;
            try
            {
                db = int.Parse(CurrentProbeDb.Text);
                if (db < 0 || db > 100) throw new Exception();
            }
            catch (Exception)
            {
                MessageBox.Show($"无效的db值{CurrentProbeDb.Text}");
                CurrentSampleDelay.Text = CurrentProbeInfo.Config.db.ToString();
                return;
            }
            CurrentProbeInfo.Config.db = db;
            CurrentProbeInfo.SetChannelConfig(CurrentProbeInfo.Config);
            UpdateCurrentPrboeInfo(CurrentProbeInfo);
        }
    }
}
