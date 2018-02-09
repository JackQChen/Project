using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
    [Flags]
    public enum Direction
    {
        None = 1 << 0,
        Front = 1 << 1,
        Back = 1 << 2,
        Left = 1 << 3,
        Right = 1 << 4,
        Up = 1 << 5,
        Down = 1 << 6,
    }
}
