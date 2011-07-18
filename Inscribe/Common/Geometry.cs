using System;
using System.Windows;

namespace Inscribe.Common
{
    public static class Geometry
    {
        public static double DistanceDouble(this Point arg1, Point arg2)
        {
            var x = arg1.X - arg2.X;
            var y = arg1.Y - arg2.Y;
            return x * x + y * y;
        }

        public static double Distance(this Point arg1, Point arg2)
        {
            return Math.Sqrt(DistanceDouble(arg1, arg2));
        }
    }
}
