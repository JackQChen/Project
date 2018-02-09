using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public struct PerspectiveParam
    {
        public PerspectiveParam(double fovy, double aspect, double zNear, double zFar)
        {
            this.fovy = fovy; this.aspect = aspect;
            this.zNear = zNear; this.zFar = zFar;
        }
        public PerspectiveParam(decimal fovy, decimal aspect, decimal zNear, decimal zFar)
        {
            this.fovy = (double)fovy; this.aspect = (double)aspect;
            this.zNear = (double)zNear; this.zFar = (double)zFar;
        }

        public double fovy;
        public double aspect;
        public double zNear;
        public double zFar;
        
        public override string ToString()
        {
            return string.Format("fovy:{0:f2},aspect:{1:f2},zNear:{2:f2},zFar:{3:f2}"
                , fovy, aspect, zNear, zFar);
        }
    }
}
