using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    public class Led:Gate
    {
        private Dot input;

        public Led()
        {
            input = new Dot(new Point(-10, 15),this);
        }

        public override void Simulate()
        {
            if (input.w != null)
                input.Value = input.w.GetOtherDot(input).Value;
        }

        public Led(Point location) : this()
        {
            Location = location;
        }

        public override void Draw(Graphics g)
        {
            int x, y, w, h;
            x = Location.X; y=Location.Y;
            w = 30; h = 30;
            Size = new Size(30, 30);
            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            g.DrawEllipse(
                Selected ? Pens.Blue : Pens.Black,
                new Rectangle(0, 0, w, h));

            g.FillEllipse(input.Value ? Brushes.Yellow : Brushes.Black,
                new Rectangle(2, 2, w - 4, h - 4));
            g.DrawLine(Pens.Black,
                new Point(- 10, 15), new Point(0, 15));
            input.Draw(g);

            g.RotateTransform(-Angle);
            g.TranslateTransform(-x, -y);
        }

        public bool GetValue()
        {
            return input.Value;
        }

        public override Dot DotOn(Point p)
        {
            if (Math.Abs(p.X - input.Location.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - input.Location.Y) <= Settings.Default.DotRadius)
                return input;
            return null;
        }

        public override string ToString()
        {
            return "Led (" + Location.X + ',' + Location.Y + ")";
        }
    }
}
