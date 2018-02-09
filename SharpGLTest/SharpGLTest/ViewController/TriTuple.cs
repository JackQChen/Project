using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL.SceneGraph;

namespace Test
{
    public struct TriTuple
    {
        public double X;
        public double Y;
        public double Z;
        public TriTuple(double x, double y, double z)
        {
            // TODO: Complete member initialization
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public override string ToString()
        {
            return string.Format("{0:f2},{1:f2},{2:f2}", X, Y, Z);
        }

        internal void Add(TriTuple diff)
        {
            this.X = this.X + diff.X;
            this.Y = this.Y + diff.Y;
            this.Z = this.Z + diff.Z;
        } 
    }
}
