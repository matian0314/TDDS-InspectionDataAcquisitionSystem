using MathNet.Numerics.Statistics;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace 探伤算法
{
    public class InspectionManager
    {
        /// <summary>
        /// 中间结果保存的地址
        /// </summary>
        private static string IntermediateDirectory { get; set; } = ConfigurationManager.AppSettings["IntermediatePath"];
        private static string CurrentDirectory { get; set; }
        private static InspectSettings CurrentInspectSettings { get; set; }
        public static InspectionResult Inspect(ProbeDataInfo info, string name, List<byte[]> pic)
        {
            var newPic = pic.Select(p => p.ToList()).ToList();
            return Inspect(info, name, newPic);
        }
        public static InspectionResult Inspect(ProbeDataInfo info, string name, List<List<byte>> pic)
        {
            //处理结果
            InspectionResult result = new InspectionResult();
            
            CurrentDirectory = Path.Combine(IntermediateDirectory, DateTime.Now.ToString("yyyy年MM月dd日"), name, info.ToBriefString());
            Directory.CreateDirectory(CurrentDirectory);
            result.IntermediateFolder = CurrentDirectory;



            CurrentInspectSettings = GetSettings(info.Type);
            pic = InitializeData(pic, info.Gain, CurrentInspectSettings);
            BitmapHelper.SaveImage(CurrentDirectory, $"原始图片", pic);
            using (ResourcesTracker tracker = new ResourcesTracker())
            {
                //采用ImreadModes.Color模式下，MatType =  MatType.CV_8UC3
                //采用ImreadModes.Grayscale，MatType = MatType.CV_8UC1
                Mat mat = tracker.T(new Mat(Path.Combine(CurrentDirectory, "原始图片.bmp"), ImreadModes.Grayscale));
                //进行滤波
                Mat filtered = FilterImage(tracker, mat);
                SaveIntermediateImage(filtered, "滤波");
                //去除不关心部分数据
                Mat masked = MaskImage(info, tracker, filtered);
                SaveIntermediateImage(masked, "掩膜");
                //对数据做二值化,采用默认的阈值
                Mat binary = Binaryzation(tracker, masked, 0);
                SaveIntermediateImage(binary, "二值化");
                //对数据做开操作，去除幻象波
                Mat open = MorphologyEx(tracker, binary);
                SaveIntermediateImage(open, "开操作");
                //获取x,y方向上的梯度
                Mat sobelX = tracker.NewMat();
                Mat sobelY = tracker.NewMat();
                //对X方向上求一阶导数
                Cv2.Sobel(mat, sobelX, MatType.CV_16SC1, 1, 0);
                //对Y方向上求一阶导数
                Cv2.Sobel(mat, sobelY, MatType.CV_16SC1, 0, 1);
                SaveIntermediateImage(sobelX, "梯度X");
                SaveIntermediateImage(sobelY, "梯度Y");
                //获取连通域
                Mat connect, stats, centroids;
                int regionCount = FindConnectedRegion(tracker, open, out connect, out stats, out centroids);

                for (int i = 1; i < regionCount; i++)
                {
                    //如果连通区域太小，放弃
                    DefectInfo defect = AnalyzeConnectedRegion(tracker, info, mat, connect, stats, centroids, i, sobelX, sobelY);
                    if(defect != null)
                    {
                        result.DefectInfos.Add(defect);
                    }
                }

            }
            result.DefectInfos = result.DefectInfos.OrderByDescending(d => d.Area).ToList();
            return result;
        }

        private static Mat MorphologyEx(ResourcesTracker tracker, Mat binary)
        {
            Mat open = tracker.NewMat();
            Cv2.MorphologyEx(binary, open, MorphTypes.Open, null);
            return open;
        }

        private static DefectInfo AnalyzeConnectedRegion(ResourcesTracker tracker, ProbeDataInfo info, Mat mat, Mat connect, Mat stats, Mat centroids,int labelIndex, Mat sobelx, Mat sobely)
        {
            //忽略过小的连通域
            if (stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Area) < 20) return null;

            DefectInfo defect = new DefectInfo(info);
            defect.XMin = stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Left);
            defect.YMin = stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Top);
            defect.Width = stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Width);
            defect.Height = stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Height);
            defect.Area = stats.At<int>(labelIndex, (int)ConnectedComponentsTypes.Area);

            defect.CentralX = (double)centroids.At<double>(labelIndex, 0).Round(2);
            defect.CentralY = (double)centroids.At<double>(labelIndex, 1).Round(2);

            //计算以下数据
            //最亮处亮度
            byte max = 0;
            //x方向上的梯度
            defect.GratitudeX = new int[(int)defect.Height];
            defect.MaxGratitudeX = new int[defect.Height];
            defect.GratitudeXCount = new int[(int)defect.Height];
            //y方向上的梯度
            defect.GratitudeY = new int[(int)defect.Width];
            defect.MaxGratitudeY = new int[(int)defect.Width];
            defect.GratitudeYCount = new int[(int)defect.Width];
            //平均亮度
            //最亮的25个点的平均亮度
            List<int> grayLevel = new List<int>();
            
            for (int x = (int)defect.XMin; x <= (int)defect.XMax; x++)
            {
                for (int y = (int)defect.YMin; y <= (int)defect.YMax; y++)
                {
                    if(connect.At<int>(y, x) != labelIndex)
                    {
                        continue;
                    }
                    byte gray = mat.At<byte>(y, x);
                    if(gray > max)
                    {
                        max = gray;
                    }
                    //更新梯度
                    //X梯度
                    //梯度变化太小的点忽略不计
                    if (Math.Abs(sobelx.At<short>(y, x)) > 100)
                    {
                        defect.GratitudeX[y - (int)defect.YMin] += sobelx.At<short>(y, x);
                        defect.GratitudeXCount[y - (int)defect.YMin]++;
                    }
                    
                    if(Math.Abs(defect.MaxGratitudeX[y - (int)defect.YMin]) < Math.Abs(sobelx.At<short>(y, x)))
                    {
                        defect.MaxGratitudeX[y - (int)defect.YMin] = sobelx.At<short>(y, x);
                    }

                    //Y梯度
                    //梯度变化太小的点忽略不计
                    if (Math.Abs(sobely.At<short>(y, x)) > 100)
                    {
                        defect.GratitudeY[x - (int)defect.XMin] += sobely.At<short>(y, x);
                        defect.GratitudeYCount[x - (int)defect.XMin]++;
                    }

                    if (Math.Abs(defect.MaxGratitudeY[x - (int)defect.XMin]) < Math.Abs(sobely.At<short>(y, x)))
                    {
                        defect.MaxGratitudeY[x - (int)defect.XMin] = sobely.At<short>(y, x);
                    }

                    //平均亮度
                    //最亮的10%的亮度
                    grayLevel.Add(gray);
                }
            }

            for (int i = 0; i < defect.GratitudeXCount.Length; i++)
            {
                if(defect.GratitudeXCount[i] != 0)
                {
                    defect.GratitudeX[i] /= defect.GratitudeXCount[i];
                }
            }
            for (int i = 0; i < defect.GratitudeYCount.Length; i++)
            {
                if (defect.GratitudeYCount[i] != 0)
                {
                    defect.GratitudeY[i] /= defect.GratitudeYCount[i];
                }
            }
            defect.Average = ArrayStatistics.MeanStandardDeviation(grayLevel.ToArray()).Mean;
            defect.StandardDiavation = ArrayStatistics.MeanStandardDeviation(grayLevel.ToArray()).StandardDeviation;
            defect.Average25 = grayLevel.OrderByDescending(g => g).Take(25).Average();

            Mat color = tracker.NewMat();
            Cv2.CvtColor(mat, color, ColorConversionCodes.GRAY2RGB);
            color.Rectangle(new Rect(defect.XMin, defect.YMin, defect.Width, defect.Height), Scalar.Red, thickness: 2);

            SaveIntermediateImage(color, $"连通域{labelIndex}");

            return defect;
        }

        private static int FindConnectedRegion(ResourcesTracker tracker, Mat open, out Mat ConnectedComponets, out Mat stats, out Mat centroid)
        {

            //快速连通域分析只支持单通道
            //初始化变量
            stats = tracker.NewMat();
            centroid = tracker.NewMat();
            ConnectedComponets = tracker.NewMat(open.Size(), MatType.CV_32SC1, Scalar.Black);

            int regionCount = Cv2.ConnectedComponentsWithStats(open, ConnectedComponets, stats, centroid, PixelConnectivity.Connectivity8, ConnectedComponets.Type());
            return regionCount;
        }

        private static InspectSettings GetSettings(string type)
        {
            InspectSettings setting = null;
            switch (type)
            {
                case "斜探头":
                    setting = InspectSettings.AngleProbe;
                    break;
                case "斜探头-轮缘":
                    setting = InspectSettings.FlangeProbe;
                    break;
                case "直探头":
                default:
                    setting = InspectSettings.StraightProbe;
                    break;
            }

            return setting;
        }

        private static List<List<byte>> InitializeData(List<List<byte>> pic, double gain, InspectSettings setting)
        {
            if (setting.UseEnvolope)
            {
                for (int i = 0; i < pic.Count; i++)
                {
                    pic[i] = GetEnvolope(pic[i], setting.EnvolopeKSize);
                }
            }
            if (setting.UseGainCompensation)
            {
                for (int i = 0; i < pic.Count; i++)
                {
                    pic[i] = GetCompensation(pic[i], setting.CompensationValue - gain);
                }
            }
            return pic;
        }

        private static List<byte> GetEnvolope(List<byte> InitialWave, int ksize)
        {
            var result = new List<byte>();
            int left = (ksize - 1) / 2;
            int right = ksize - 1 - left;
            for (int i = 0; i < InitialWave.Count; i++)
            {
                //正常情况
                if (i - left >= 0 && i + right < InitialWave.Count)
                {
                    result.Add(InitialWave.Skip(i - left).Take(ksize).Max());
                }
                else if (i - left < 0)
                {
                    result.Add(InitialWave.Take(ksize).Max());
                }
                else//i + right >= InitialWave.Count
                {
                    result.Add(InitialWave.Skip(InitialWave.Count - ksize).Take(ksize).Max());
                }
            }
            return result;
        }
        private static List<byte> GetCompensation(List<byte> InitialWave, double compensation)
        {
            var ajustment = Math.Pow(10, compensation / 20);
            for (int i = 0; i < InitialWave.Count; i++)
            {
                if(InitialWave[i] * ajustment > 255)
                {
                    InitialWave[i] = 255;
                }
                else
                {
                    InitialWave[i] = (byte)(ajustment * InitialWave[i]);
                }
            }
            return InitialWave;
            
        }
        private static bool SaveIntermediateImage(Mat mat, string ImageType)
        {
            return mat.SaveImage(Path.Combine(CurrentDirectory, $"{ImageType}.bmp"));
        }
        private static Mat MaskImage(ProbeDataInfo ProbeDataInfo, ResourcesTracker tracker, Mat mat)
        {
            Mat mask = tracker.NewMat(new OpenCvSharp.Size(mat.Width, mat.Height), mat.Type(), Scalar.Black);
            int start = (int)(ProbeDataInfo.EffectiveSoundPathLow / ProbeDataInfo.PointInterval);
            int length = (int)((ProbeDataInfo.EffectiveSoundPathHigh - ProbeDataInfo.EffectiveSoundPathLow) / ProbeDataInfo.PointInterval);
            mask.Rectangle(new Rect(start, 0, length, mat.Height), Scalar.White, thickness: -1/*thickness为-1代表填满*/);

            Mat maskedImage = tracker.NewMat();
            Cv2.BitwiseAnd(mat, mask, maskedImage);

            return maskedImage;
        }
        private static Mat Binaryzation(ResourcesTracker tracker, Mat maskedImage, double Threshold = 0)
        {
            if(Threshold <= 0)
            {
                Threshold = CurrentInspectSettings.BinarizationThreshold;
            }
            if(Threshold > 100)
            {
                Threshold = 100;
            }
            Mat binary = tracker.NewMat();
            Cv2.Threshold(maskedImage, binary, 2.55 * Threshold, 255, ThresholdTypes.Binary);
            return binary;
        }
        private static Mat FilterImage(ResourcesTracker tracker, Mat mat)
        {
            //具体滤波算法再研究
            return mat;
        }
        public static string CallPythonScript(string passTime)
        {
            string interpreter = ConfigurationManager.AppSettings["PythonInterpreter"];
            string scriptPath = ConfigurationManager.AppSettings["PythonScriptPath"];
            //参数1 文件存储的根目录
            string sourceDir = ConfigurationManager.AppSettings["StoragePath"];
            //参数2 过车时间/探伤文件存储的子文件夹 passTime
            string destFilePath = Path.Combine(sourceDir, interpreter, $"{passTime}_探伤结果.txt");
            CmdHelper.RunPythonScript(interpreter, scriptPath, sourceDir, passTime, destFilePath);
            return destFilePath;
        }
    }
}
