using System;
using System.Collections.Generic;
using PhysicsEngine.Common;

namespace PhysicsEngine.Base
{
    public static class Factory
    {
        public static Body CreateRectangleBody(double x, double y, double width, double height, bool isStatic = false)
        {
            var path = new List<Point> { new Point(0, 0), new Point(width, 0), new Point(width, height), new Point(0, height) };
            var body = new Body { Static = isStatic };
            body.Init(path);
            body.Position = new Point(x, y);
            return body;
        }
        public static Body CreateCircleBody(double x, double y, double r, bool isStatic = false)
        {
            var path = new List<Point>();
            for (var i = 0; i < 2 * r; i++)
                path.Add(new Point(i, r - Math.Sqrt((2 * r - i) * i)));
            for (var i = 2 * r; i > 0; i--)
                path.Add(new Point(i, r + Math.Sqrt((2 * r - i) * i)));
            var body = new Body { Static = isStatic };
            body.Init(path);
            body.Position = new Point(x, y);
            return body;
        }
    }
}
