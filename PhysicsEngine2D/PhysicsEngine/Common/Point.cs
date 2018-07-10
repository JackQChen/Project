using System;

namespace PhysicsEngine.Common
{
    public class Point
    {
        public double X { get; set; }

        public double Y { get; set; }

        public Point()
        {
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        private Point(Point pt)
        {
            X = pt.X;
            Y = pt.Y;
        }

        public override string ToString()
        {
            return "{" + X + "," + Y + "}";
        }

        public static Point operator +(Point lhs, Point rhs)
        {
            var pt = new Point(lhs);
            pt.X += rhs.X;
            pt.Y += rhs.Y;
            return pt;
        }

        public static Point operator -(Point lhs, Point rhs)
        {
            var pt = new Point(lhs);
            pt.X -= rhs.X;
            pt.Y -= rhs.Y;
            return pt;
        }

        public static Point operator *(Point lhs, double rhs)
        {
            var pt = new Point(lhs);
            pt.X *= rhs;
            pt.Y *= rhs;
            return pt;
        }

        public static Point operator *(double lhs, Point rhs)
        {
            var pt = new Point(rhs);
            pt.X *= lhs;
            pt.Y *= lhs;
            return pt;
        }

        public static Point operator /(Point lhs, double rhs)
        {
            var pt = new Point(lhs);
            pt.X /= rhs;
            pt.Y /= rhs;
            return pt;
        }

        public static Point operator /(Point lhs, Point rhs)
        {
            var pt = new Point(lhs);
            pt.X /= rhs.X;
            pt.Y /= rhs.Y;
            return pt;
        }

        public void Offset(Point pt)
        {
            X += pt.X;
            Y += pt.Y;
        }

        public void Clear()
        {
            X = Y = 0;
        }

        /// <summary>
        /// 长度
        /// </summary>
        /// <returns></returns>
        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        /// <summary>
        /// 标准化
        /// </summary>
        public void Normalize()
        {
            var magnitude = Math.Sqrt(X * X + Y * Y);
            if (Math.Sign(magnitude) == 0)
            {
                X = Y = 0;
                return;
            }
            X /= magnitude;
            Y /= magnitude;
        }

        /// <summary>
        /// 角度
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public double Angle(Vertex vertex)
        {
            return Math.Atan2(vertex.Y - Y, vertex.X - X);
        }

        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="point"></param>
        public void RotateAbout(double angle, Point point)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);
            var x = point.X + ((X - point.X) * cos - (Y - point.Y) * sin);
            Y = point.Y + ((X - point.X) * sin + (Y - point.Y) * cos);
            X = x;
        }
    }
}
