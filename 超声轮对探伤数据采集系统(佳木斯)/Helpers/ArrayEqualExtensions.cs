using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class ArrayEqualExtensions
    {
        public static bool IsArrayEqual<T>(this T[] a, T[] b) where T :struct
        {
            if(a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if(!a[i].Equals(b[i]) )
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsArrayEqual<T>(this T[] a, T[] b, Func<T,T, bool> func)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (!func(a[i],b[i]))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsArrayEqual<T>(this T[,] a, T[,] b, int X, int Y)
        {
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    if (!a[i, j].Equals(b[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool IsArrayEqual<T>(this T[,] a, T[,] b, int X, int Y, Func<T, T, bool> func)
        {
            for (int i = 0; i < X; i++)
            {
                for (int j = 0; j < Y; j++)
                {
                    if (!func(a[i, j],b[i, j]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
