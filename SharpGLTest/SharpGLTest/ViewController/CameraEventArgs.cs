using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    public class CameraEventArgs : EventArgs
    {
        public CameraEventArgs(TriTuple eye, TriTuple center, TriTuple up, PerspectiveParam perspective)
        {
            this.eye = eye; this.center = center; this.up = up;
            this.perspective = perspective;
        }
        public TriTuple eye;
        public TriTuple center;
        public TriTuple up;
        public PerspectiveParam perspective;
    }
}
