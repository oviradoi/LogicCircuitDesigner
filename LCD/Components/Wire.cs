using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LCD.Components
{
    [Serializable]
    public class Wire
    {
        public Dot src { get; set; }
        public Dot dst { get; set; }
        public bool Selected { get; set; }
        public List<WirePoint> Points { get; set; }

        public Wire()
        {
            Points = new List<WirePoint>();
        }

        public Dot GetOtherDot(Dot d)
        {
            if (d == src)
                return dst;
            if (d == dst)
                return src;
            return null;
        }
    }
}
