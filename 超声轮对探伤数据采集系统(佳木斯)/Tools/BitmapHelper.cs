using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Tools
{
    public static class BitmapHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName">不包含文件后缀</param>
        /// <param name="data"></param>
        /// <param name="func">第一个参数表示从第几组数据，第二个参数代表第几个点，第三个参数代表原始数据，返回灰度值</param>
        public static string SaveImage(string directory, string fileName, List<byte[]> data, bool overWrite = false, Func<int, int, List<byte[]>, byte> func = null)
        {
            string path = Path.Combine(directory, $"{fileName}.bmp");
            if (File.Exists(path) && !overWrite) return path;
            if (func == null)
            {
                func = (y, x, d) => data[y][x];
            }
            var height = data.Count;
            var width = data.Max(list => list.Length);
            //无论对于斜探头还是直探头，删除波高小于20%的波
            Bitmap map = new Bitmap(width, height);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    byte grayLevel = func(y, x, data);
                    map.SetPixel(x, y, Color.FromArgb(grayLevel, grayLevel, grayLevel));
                }
            }
            Directory.CreateDirectory(directory);
            map.Save(path);
            return path;
        }
        public static string SaveImage(string directory, string fileName, List<List<byte>> data, bool overWrite = false, Func<int, int, List<byte[]>, byte> func = null)
        {
            List<byte[]> adata = new List<byte[]>();
            foreach(var d in data)
            {
                adata.Add(d.ToArray());
            }
            return SaveImage(directory, fileName, adata, overWrite, func);
        }
        /// <summary>
        /// 读取二值化后的图片,返回逻辑1的点
        /// </summary>
        /// <returns></returns>
        public static List<Point> ReadBinaryImage(string path, byte Threshold = 128)
        {
            List<Point> points = new List<Point>();
            Bitmap map = new Bitmap(path);
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    Color color = map.GetPixel(x, y);
                    byte grey = Convert.ToByte((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    if(grey >= 128)
                    {
                        points.Add(new Point(x, y));
                    }
                }
            }
            return points;
        }
    }
}
