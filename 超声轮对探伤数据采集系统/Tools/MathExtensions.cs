using System;
using System.Linq;

namespace Tools
{
    public static class MathHelper
    {
        /// <summary>
        /// 将double类型数据转换为decimal类型，并取digits位小数
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="digits">取小数位数</param>
        /// <returns></returns>
        public static decimal Round(this double Value, int digits = 2)
        {
            return (decimal)Math.Round(Value, digits);
        }
        /// <summary>
        /// 将float类型数据转换为decimal类型，并取digits位小数
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="digits">取小数位数</param>
        /// <returns></returns>
        public static decimal Round(this float Value, int digits = 2)
        {
            return (decimal)Math.Round(Value, digits);
        }
        /// <summary>
        /// 取T类型的最大值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T Max<T>(params T[] values)
        {
            return values.Max();
        }
        /// <summary>
        /// 取T类型的最小值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        public static T Min<T>(params T[] values)
        {
            return values.Min();
        }
        /// <summary>
        /// 将角度制的度°改成弧度制的rad
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double DegreeToRad(double degree)
        {
            return Math.PI * degree / 180;
        }
        /// <summary>
        /// 将弧度制的rad转变为角度制的°
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double RadToDegree(double rad)
        {
            return rad * 180 / Math.PI;
        }
        /// <summary>
        /// 采用弧度制
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public static double Sin(double degree)
        {
            return Math.Sin(DegreeToRad(degree));
        }
        public static double Cos(double degree)
        {
            return Math.Cos(DegreeToRad(degree));
        }
        public static double Tan(double degree)
        {
            return Math.Tan(DegreeToRad(degree));
        }
        public static double Asin(double value)
        {
            return RadToDegree(Math.Asin(value));
        }
        public static double Acos(double value)
        {
            return RadToDegree(Math.Acos(value));
        }
        public static double ATan(double value)
        {
            return RadToDegree(Math.Atan(value));
        }
        /// <summary>
        /// 求解一元二次方程的根
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static (double, double) SolveQuadraticEquation(double a, double b, double c)
        {
            if (a == 0) return (-1 * b / c, -1 * b / c);
            double delta = Math.Pow(b, 2) - 4 * a * c;
            if (delta < 0) return (double.NaN, double.NaN);
            else
            {
                double x1 = (-1 * b + Math.Sqrt(delta)) / (2 * a);
                double x2 = (-1 * b - Math.Sqrt(delta)) / (2 * a);
                return (x1, x2);
            }
        }
    }
}