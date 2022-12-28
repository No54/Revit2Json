using Autodesk.Revit.DB;
using System;

namespace Rvt2Json.App.Models
{
    public class PointInt : IComparable<PointInt>
    {
        public long X { get; set; }
        public long Y { get; set; }
        public long Z { get; set; }

        /// <summary>
        /// 小于此值，则考虑为0 
        /// </summary>
        const double _eps = 1.0e-9;

        /// <summary>
        /// 英尺转为毫米的转换系数
        /// </summary>
        const double _feet_to_mm = 25.4 * 12;

        /// <summary>
        /// 将一个给定的英尺转为毫米 
        /// </summary>
        static long ConvertFeetToMillimetres(double d)
        {
            if (0 < d)
            {
                return _eps > d
                  ? 0
                  : (long)(_feet_to_mm * d + 0.5);

            }
            else
            {
                return _eps > -d
                  ? 0
                  : (long)(_feet_to_mm * d - 0.5);

            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="p"></param>
        /// <param name="switch_coordinates">是否转换坐标，YZ交换，X取逆</param>
        public PointInt(XYZ p, bool switch_coordinates)
        {
            X = ConvertFeetToMillimetres(p.X);
            Y = ConvertFeetToMillimetres(p.Y);
            Z = ConvertFeetToMillimetres(p.Z);

            if (switch_coordinates)
            {
                X = -X;
                long tmp = Y;
                Y = Z;
                Z = tmp;
            }
        }

        public int CompareTo(PointInt a)
        {
            long d = X - a.X;

            if (0 == d)
            {
                d = Y - a.Y;

                if (0 == d)
                {
                    d = Z - a.Z;
                }
            }
            return (0 == d) ? 0 : ((0 < d) ? 1 : -1);
        }
    }
}
