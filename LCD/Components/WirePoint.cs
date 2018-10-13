using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace LCD.Components
{
    [Serializable]
    public class WirePoint
    {
        public Point Location;
        public int X
        {
            get
            {
                return Location.X;
            }
            set
            {
                Location.X = value;
            }
        }

        public int Y
        {
            get
            {
                return Location.Y;
            }
            set
            {
                Location.Y = value;
            }
        }

        public WirePoint(){}
        public WirePoint(int x,int y)
        {
            Location.X = x;
            Location.Y = y;
        }
        public WirePoint(Point p)
        {
            Location = p;
        }

        public bool PointOn(Point p)
        {
            if (Math.Abs(p.X - Location.X) <= Properties.Settings.Default.WirePointRadius && Math.Abs(p.Y - Location.Y) <= Properties.Settings.Default.WirePointRadius)
                return true;
            return false;
        }

        public override string ToString()
        {
            return Location.ToString();
        }
    }
}
