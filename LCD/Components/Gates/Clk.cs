using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    public class Clk : Gate
    {
        private Dot output;

        [Browsable(true)]
        public int HighTime { get; set; }
        [Browsable(true)]
        public int LowTime { get; set; }

        private DateTime lastTime;

        public override void Draw(System.Drawing.Graphics g)
        {
            int x, y, w, h;
            w = 30; h = 30;
            x = Location.X; y=Location.Y;
            Size = new Size(w, h);
            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            //g.FillRectangle(Brushes.Gray, new Rectangle(0, 0, w, h));
            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, w, h));
            g.DrawLine(Pens.Black, new Point(w, h / 2), new Point(w + 10, h / 2));

            g.DrawLine(Pens.Black, w / 2, 4, w / 2, h / 2 - 3);
            g.DrawLine(Pens.Black, w / 2, h/2+3, w / 2, h - 4);
            g.DrawLine(Pens.Black, 4, h / 2, w / 2 - 3, h / 2);
            g.DrawLine(Pens.Black, w / 2 + 3, h / 2, w - 4, h / 2);

            g.DrawLine(Pens.Gray, 8, 8, w / 2 - 3, h / 2 - 3);
            g.DrawLine(Pens.Gray, w - 8, h - 8, w / 2 + 3, h / 2 + 3);
            g.DrawLine(Pens.Gray, 8, h-8, w / 2 - 3, h / 2 + 3);
            g.DrawLine(Pens.Gray, w - 8, 8, w / 2 + 3, h / 2 - 3);

            g.DrawEllipse(Pens.Black, new Rectangle(2, 2, w-4, h-4));

            output.Draw(g);
            if (Selected)
                g.DrawRectangle(Pens.Blue, new Rectangle(0,0, Size.Width,Size.Height));

            g.RotateTransform(-Angle);
            g.TranslateTransform(-x, -y);
        }

        public override void Reset()
        {
            output.Value = false;
        }

        public override void Simulate()
        {
            if (lastTime == null)
                lastTime = DateTime.Now;
            if(output.Value==false && DateTime.Now.Subtract(lastTime).TotalMilliseconds > LowTime)
            {
                output.Value = true;
                lastTime = DateTime.Now;
            }
            if (output.Value == true && DateTime.Now.Subtract(lastTime).TotalMilliseconds > HighTime)
            {
                output.Value = false;
                lastTime = DateTime.Now;
            }
        }

        public Clk(Point loc)
        {
            Location = loc;
            output = new Dot(new Point(40,15),this);
            HighTime = LowTime = 1000;
        }

        public override Dot DotOn(Point p)
        {
            if ((Math.Abs(p.X - (Size.Width + 10)) <= Settings.Default.DotRadius) && (Math.Abs(p.Y - (Size.Height / 2)) <= Settings.Default.DotRadius))
                return output;
            return null;
        }

        public override string ToString()
        {
            return "Btn (" + Location.X + "," + Location.Y + ")";
        }
    }
}
