using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    public class Not : Gate
    {
        private Dot input { get; set; }
        private Dot output { get; set; }

        public override void Simulate()
        {
            if (input.w != null)
                input.Value = input.w.GetOtherDot(input).Value;
            output.Value = !input.Value;
        }

        public override void Draw(Graphics g)
        {
            int w, h, x, y;
            x = Location.X; y = Location.Y;
            w = 50; h = 24;
            Size = new Size(w, h);

            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            g.DrawLine(Pens.Black, new Point(w-13, h / 2 - 0), new Point(w, h / 2 - 0));
            g.DrawLine(Pens.Black, new Point(0, h / 2), new Point(10, h / 2));

            Point[] vp = new Point[3];
            vp[0].X = 10; vp[0].Y = 0;
            vp[1].X = 10; vp[1].Y = h;
            vp[2].X = w - 20; vp[2].Y = h / 2;
            g.DrawPolygon(Pens.Black, vp);

            g.DrawEllipse(Pens.Black, new Rectangle(w - 17 - 3, h / 2 - 3, 6, 6));
            input.Draw(g);
            output.Draw(g);

            if (Selected)
                g.DrawRectangle(Pens.Blue, new Rectangle(0, 0, w, h));

            g.RotateTransform(-Angle);
            g.TranslateTransform(-x, -y);
        }

        public override void Reset()
        {
            output.Value = false;
        }

        private Not()
        {
            input = new Dot(new Point(0,12),this);
            output = new Dot(new Point(50, 12),this);
        }

        public Not(Point loc):this()
        {
            Location = loc;
        }

        public override Dot DotOn(Point p)
        {
            if (Math.Abs(p.X - input.Location.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - input.Location.Y) <= Settings.Default.DotRadius)
                return input;

            if (Math.Abs(p.X - output.Location.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - output.Location.Y) <= Settings.Default.DotRadius)
                return output;
            return null;
        }

        public override string ToString()
        {
            return "Not (" + Location.X + "," + Location.Y + ")";
        }
    }
}
