using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace 探伤算法
{
    public class InspectSettings
    {
        private static InspectSettings _straight;
        private static InspectSettings _angle;
        private static InspectSettings _flange;
        /// <summary>
        /// 直探头配置
        /// </summary>
        public static InspectSettings StraightProbe
        {
            get => _straight;
            set
            {
                _straight = value;
                _straight.Update("直探头");
            }
        }
        public static InspectSettings AngleProbe
        {
            get => _angle;
            set
            {
                _angle = value;
                _angle.Update("斜探头");
            }
        }
        public static InspectSettings FlangeProbe
        {
            get => _flange;
            set
            {
                _flange = value;
                _flange.Update("轮缘探头");
            }
        }
        static InspectSettings()
        {
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configDir = Path.Combine(baseDir, "探伤算法");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            if(!File.Exists(Path.Combine(configDir, $"直探头.cfg")))
            {
                CreateDefaultConfig("直探头");
            }
            if (!File.Exists(Path.Combine(configDir, $"斜探头.cfg")))
            {
                CreateDefaultConfig("斜探头");
            }
            if (!File.Exists(Path.Combine(configDir, $"轮缘探头.cfg")))
            {
                CreateDefaultConfig("轮缘探头");
            }
            _straight = JsonConvert.DeserializeObject<InspectSettings>(FileHelper.ReadFile(Path.Combine(configDir, $"直探头.cfg")));
            _angle = JsonConvert.DeserializeObject<InspectSettings>(FileHelper.ReadFile(Path.Combine(configDir, $"斜探头.cfg")));
            _flange = JsonConvert.DeserializeObject<InspectSettings>(FileHelper.ReadFile(Path.Combine(configDir, $"轮缘探头.cfg")));
        }

        private static void CreateDefaultConfig(string type)
        {
            InspectSettings settings = new InspectSettings();
            switch(type)
            {
                case "直探头":
                    settings.UseEnvolope = true;
                    settings.EnvolopeKSize = 5;
                    settings.BinarizationThreshold = 40;
                    settings.UseGainCompensation = false;
                    settings.CompensationValue = 45;
                    break;
                case "斜探头":
                    settings.UseEnvolope = false;
                    settings.EnvolopeKSize = 1;
                    settings.BinarizationThreshold = 40;
                    settings.UseGainCompensation = false;
                    settings.CompensationValue = 60;
                    break;
                case "轮缘探头":
                    settings.UseEnvolope = false;
                    settings.EnvolopeKSize = 1;
                    settings.BinarizationThreshold = 40;
                    settings.UseGainCompensation = false;
                    settings.CompensationValue = 65;
                    break;
            }
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string configDir = Path.Combine(baseDir, "探伤算法");
            FileHelper.CreateFile(Path.Combine(configDir, $"{type}.cfg"), JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// 是否对探头进行包络
        /// </summary>
        public bool UseEnvolope { get; set; }
        /// <summary>
        /// 如果使用包络进行处理，Ksize大小
        /// </summary>
        public int EnvolopeKSize { get; set; }
        /// <summary>
        /// 二值化的阈值
        /// </summary>
        public double BinarizationThreshold { get; set; }
        /// <summary>
        /// 是否对增益值进行调整
        /// </summary>
        public bool UseGainCompensation { get; set; }
        /// <summary>
        /// 调整增益值到多少
        /// </summary>
        public double CompensationValue { get; set; }
        public void Update(string name)
        {
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string fileName = Path.Combine(baseDir, "探伤算法", $"{name}.cfg");
            FileHelper.CreateFile(fileName, JsonConvert.SerializeObject(this));
        }
    }
}
