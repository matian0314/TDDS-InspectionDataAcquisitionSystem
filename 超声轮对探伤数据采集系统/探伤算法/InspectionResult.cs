using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤算法
{
    public class InspectionResult
    {
        public string IntermediateFolder { get; set; }
        public List<DefectInfo> DefectInfos { get; set; } = new List<DefectInfo>(); 
        public int AlarmLevel
        {
            get
            {
                if (DefectInfos.Count == 0) return 1;
                return DefectInfos.Max(defect => defect.AlarmLevel);
            }

        }
        public string GetRemark()
        {
            if (DefectInfos.Count == 0) return "";
            return string.Join("\n", DefectInfos.Select(d => d.GetRemark()));
        }
        public string GetImagePath(ImageType type)
        {
            string pattern = "";
            switch(type)
            {
                case ImageType.Initial:
                    pattern = "原始图片.bmp";
                    break;
                case ImageType.Filter:
                    pattern = "滤波.bmp";
                    break;
                case ImageType.Mask:
                    pattern = "掩膜.bmp";
                    break;
                case ImageType.Binary:
                    pattern = "二值化.bmp";
                    break;
                case ImageType.Open:
                    pattern = "开操作化.bmp";
                    break;
                case ImageType.GradeX:
                    pattern = "梯度X.bmp";
                    break;
                case ImageType.GradeY:
                    pattern = "梯度Y.bmp";
                    break;
                case ImageType.ConnectedRegions:
                    pattern = "连通域*.bmp";
                    break;
            }
            return string.Join(",", Directory.GetFiles(IntermediateFolder, pattern));
        }
        public enum ImageType
        {
            Initial,
            Filter,
            Mask,
            Binary,
            Open,
            GradeX,
            GradeY,
            ConnectedRegions
        }
    }
}
