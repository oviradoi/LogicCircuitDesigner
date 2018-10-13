using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    public class Or : Gate
    {
        private List<Dot> inputs { get; set; }
        private Dot output { get; set; }

        public override void Simulate()
        {
            foreach (Dot d in inputs)
            {
                if (d.w != null)
                    d.Value = d.w.GetOtherDot(d).Value;
            }
            bool o = false;
            foreach (Dot d in inputs)
                o = o | d.Value;
            output.Value = o;
        }

        public override void Draw(Graphics g)
        {
            int w, h, x, y;
            int n = inputs.Capacity;
            x = Location.X; y = Location.Y;
            w = 50; h = 6 * (2 * n + 1);
            Size = new Size(w, h);

            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            for (int i = 1; i <= n; i++)
                g.DrawLine(Pens.Black, new Point(0, 2 * i * 6 - 3), new Point(10, 2 * i * 6 - 3));

            g.DrawLine(Pens.Black, new Point(w - 18, h / 2 - 0), new Point(w - 1, h / 2 - 0));

            g.DrawArc(Pens.Black, new Rectangle(1, 0, 10, h - 1), -90, 180);
            g.DrawArc(Pens.Black, new Rectangle(-19, 0, 50, h - 1), -90, 180);

            foreach (Dot d in inputs)
                d.Draw(g);
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

        private Or(int ninputs)
        {
            inputs = new List<Dot>(ninputs);
            for (int i = 0; i < ninputs; i++)
            {
                inputs.Add(new Dot(new Point(0, 2 * (i + 1) * 6 - 3),this));
            }
            int x = 6 * (2 * ninputs + 1);
            output = new Dot(new Point(50-1,x/2),this);
        }

        public Or(int ninputs,Point loc):this(ninputs)
        {
            Location = loc;
        }

        public override Dot DotOn(Point p)
        {
            for (int i = 1; i <= inputs.Capacity; i++)
                if (Math.Abs(p.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - 2 * i * 6 + 3) <= Settings.Default.DotRadius)
                    return inputs[i - 1];

            if (Math.Abs(p.X - Size.Width - 1) <= Settings.Default.DotRadius && Math.Abs(p.Y - Size.Height / 2) <= Settings.Default.DotRadius)
                return output;
            return null;
        }

        public override string ToString()
        {
            return "Or (" + Location.X + "," + Location.Y + ")";
        }
    }
}
