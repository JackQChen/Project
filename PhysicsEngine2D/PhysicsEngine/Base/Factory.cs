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
            var xr = r / Math.Sqrt(2);
            var path = new List<Point> {
                new Point ( r,0  ),
                new Point ( r + xr  ,r- xr  ),
                new Point (2* r, r ),
                new Point ( r + xr  , r+ xr  ),
                new Point ( r , 2*r ),
                new Point ( r-xr , r+ xr   ),
                new Point ( 0,  r ),
                new Point ( r-xr , r- xr   ),
            };
            var body = new Body { Static = isStatic };
            body.Init(path);
            body.Position = new Point(x, y);
            return body;
        }
    }
}
