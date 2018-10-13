using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components
{
    [Serializable]
    public class Dot
    {
        public bool Value { get; set; }
        public Point Location { get; set; }
        public LCD.Components.Gates.Gate Parent { get; set; }
        public Wire w { get; set; }
        public String Description
        {
            get;
            set;
        }

        public Dot()
        {
            Parent = null;
        }

        public Dot(bool value):this()
        {
            this.Value = value;
        }

        public Dot(Point location,LCD.Components.Gates.Gate parent):this(false)
        {
            Location = location;
            Parent = parent;
        }

        public void Draw(Graphics g)
        {
            Pen onPen = new Pen(Settings.Default.DotOnColor);
            Pen offPen = new Pen(Settings.Default.DotOffColor);

            g.DrawEllipse(
                Value == true ? onPen : offPen,
                new Rectangle(Location.X - Settings.Default.DotRadius / 2,
                    Location.Y - Settings.Default.DotRadius / 2,
                    Settings.Default.DotRadius,
                    Settings.Default.DotRadius));

            SolidBrush offBrush = new SolidBrush(Settings.Default.DotOffColor);
            SolidBrush onBrush = new SolidBrush(Settings.Default.DotOnColor);

            g.FillEllipse(Value == true ? onBrush : offBrush,
                new Rectangle(Location.X - Settings.Default.DotRadius / 2,
                    Location.Y - Settings.Default.DotRadius / 2,
                    Settings.Default.DotRadius,
                    Settings.Default.DotRadius));
        }
    }
}
