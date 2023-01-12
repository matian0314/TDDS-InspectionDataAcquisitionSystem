using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 探伤算法
{
    public class ModelWheelDefect
    {
        public static List<ModelWheelDefect> ModelWheelDefects { get; set; }
        static ModelWheelDefect()
        {
            var defect1 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.径向刻槽,
                Index = 1,
                Angle = 30,
                Depth = -23,
                Length = 5,
                DetectedByProbes = true,
                DetectedByRimAngleProbes = true
            };
            var defect2 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.横孔,
                Index = 2,
                Angle = 120,
                Depth = 30,
                DepthMax = 31.5,
                DepthMin = 28.5,
                Length = 100,
                DetectedByProbes = true,
                DetectedByNormalAngleProbes = true,
                DetectedByDirectProbes = true,
                DetectedByRimAngleProbes = true
            };
            var defect3 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.踏面圆孔,
                Index = 3,
                Angle = 195,
                Depth = 3,
                Length = 12
            };
            var defect4 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.踏面圆孔,
                Index = 4,
                Angle = 197,
                Depth = 3,
                Length = 12
            };
            var defect5 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.踏面圆孔,
                Index = 5,
                Angle = 199,
                Depth = 3,
                Length = 12
            };
            //与探伤无关
            var defect6 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.踏面擦伤,
                Index = 6,
                Angle = 255,
                Depth = 0,
                Length = 0
            };
            var defect7 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.平底孔,
                Index = 7,
                Angle = 300,
                Depth = 20,
                DepthMax = 60,
                DepthMin = 20,
                Length = 50,
                DetectedByNormalAngleProbes = true,
                DetectedByRimAngleProbes = true,
                DetectedByDirectProbes = true,
                DetectedByProbes = true
            };
            var defect8 = new ModelWheelDefect()
            {
                Type = ModelWheelDefectType.踏面刻槽,
                Index = 8,
                Angle = 330,
                Depth = 4,
                Length = 25,
                DetectedByProbes = true,
                DetectedByDirectProbes = false,
                DetectedByNormalAngleProbes = true,
                DetectedByRimAngleProbes = true
            };
            ModelWheelDefects = new List<ModelWheelDefect>()
            {
                defect1, defect2, defect3, defect4, defect5, defect6, defect7, defect8
            };
        }
        public ModelWheelDefectType Type { get; set; }
        public int Index { get; set; }
        public double Angle { get; set; }
        public double Depth { get; set; }
        public double Length { get; set; }
        public double DepthMax { get; set; }
        public double DepthMin { get; set; }
        public bool DetectedByProbes { get; set; } = false;
        public bool DetectedByDirectProbes { get; set; } = false;
        public bool DetectedByNormalAngleProbes { get; set; } = false;
        public bool DetectedByRimAngleProbes { get; set; } = false;
    }
    public enum ModelWheelDefectType
    {
        径向刻槽,
        横孔,
        踏面圆孔,
        踏面擦伤,
        平底孔,
        踏面刻槽
    };
}
