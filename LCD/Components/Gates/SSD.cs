using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    class SSD:Gate
    {
        private List<Dot> inputs { get; set; }
        private int val=0;

        public override void Simulate()
        {
            val = 0;

            PropagateLinks();

            for (int i = 0; i < inputs.Capacity; i++)
            {
                if (inputs[i].Value)
                {
                    val += (1 << i);
                }
            }
        }

        private void PropagateLinks()
        {
            foreach (Dot d in inputs)
            {
                if (d.w != null)
                    d.Value = d.w.GetOtherDot(d).Value;
            }
        }

        public override void Draw(Graphics g)
        {
            int w = 40;
            int h = 54;

            Size = new Size(w, h);

            Pen p;

            if (Selected)
            {
                p = new Pen(Color.Blue);
            }
            else
            {
                p = new Pen(Color.Black);
            }

            Pen p1 = new Pen(Color.DimGray, 2f);
            Pen p2 = new Pen(Color.Pink, 2f);

            bool T, M, B, UL, LL, UR, LR;
            T = M = B = UL = LL = UR = LR = false;
            while (val >= 16) val %= 16;

            switch (val)
            {
                case -1:
                    break;
                case 0:
                    T = B = UL = LL = UR = LR = true;
                    break;
                case 1:
                    UR = LR = true;
                    break;
                case 2:
                    T = M = B = UR = LL = true;
                    break;
                case 3:
                    T = M = B = UR = LR = true;
                    break;
                case 4:
                    M = UL = UR = LR = true;
                    break;
                case 5:
                    T = M = B = UL = LR = true;
                    break;
                case 6:
                    T = M = B = UL = LL = LR = true;
                    break;
                case 7:
                    T = UR = LR = true;
                    break;
                case 8:
                    T = M = B = UL = LL = UR = LR = true;
                    break;
                case 9:
                    T = M = B = UL = UR = LR = true;
                    break;
                case 10:
                    T = M = UL = LL = UR = LR = true;
                    break;
                case 11:
                    UL = LL = M = B = LR = true;
                    break;
                case 12:
                    M = B = LL = true;
                    break;
                case 13:
                    M = B = LL = UR = LR = true;
                    break;
                case 14:
                    T = M = B = UL = LL = true;
                    break;
                case 15:
                    T = M = UL = LL = true;
                    break;
                default:
                    T = M = B = true;
                    break;
            }
            int x = Location.X;
            int y = Location.Y;

            g.TranslateTransform(Location.X, Location.Y);
            g.RotateTransform(Angle);
            

            g.DrawRectangle(p, 1, 1, w - 2, h - 2);

            g.FillRectangle(new SolidBrush(Color.Gray), 1, 1, w - 2, h - 2);

            

            g.DrawLine(T ? p2 : p1, 10 + 1, 6, 30 - 1, 6); //Top
            g.DrawLine(M ? p2 : p1, 10 + 1, 27, 30 - 1, 27); //Middle
            g.DrawLine(B ? p2 : p1, 10 + 1, 48, 30 - 1, 48); //Bottom

            g.DrawLine(UL ? p2 : p1, 10 + 1, 6, 10 + 1, 6 + 21); //UL
            g.DrawLine(LL ? p2 : p1, 10 + 1, 6 + 21, 10 + 1, 6 + 21 + 21); //LL

            g.DrawLine(UR ? p2 : p1, 10 + 20 - 1, 6, 10 + 20 - 1, 6 + 21); //UR
            g.DrawLine(LR ? p2 : p1, 10 + 20 - 1, 6 + 21, 10 + 20 - 1, 6 + 21 + 21); //LR

            
            DrawDots(g);

            g.RotateTransform(-Angle);
            g.TranslateTransform(-x, -y);
        }

        private void CreateDots()
        {
            int x = Location.X;
            int y = Location.Y;

            Dot dotFirstInput = new Dot(
                new Point(1, 11), this);
            Dot dotSecondInput = new Dot(
                new Point(1, 22), this);
            Dot dotThirdInput = new Dot(
                new Point(1, 33), this);
            Dot dotFourthInput = new Dot(
                new Point(1, 44), this);

            inputs.Add(dotFirstInput);
            inputs.Add(dotSecondInput);
            inputs.Add(dotThirdInput);
            inputs.Add(dotFourthInput);
        }

        private void DrawDots(Graphics g)
        {
            foreach (Dot dot in inputs)
            {
                dot.Draw(g);
            }
        }

        public override void Reset()
        {
            foreach (Dot dot in inputs)
            {
                dot.Value = false;
            }
        }

        public SSD(Point loc)
        {
            Location = loc;
            inputs = new List<Dot>();
            CreateDots();
        }

        public override Dot DotOn(Point p)
        {
            for (int i = 1; i <= inputs.Capacity; i++)
            {
                if (Math.Abs(p.X - inputs[i - 1].Location.X) <= Settings.Default.DotRadius &&
                    Math.Abs(p.Y - inputs[i - 1].Location.Y) <= Settings.Default.DotRadius)
                {
                    return inputs[i - 1];
                }
            }

            return null;
        }

        public override string ToString()
        {
            return "SSD (" + Location.X + "," + Location.Y + ")";
        }

        public override void MouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.MouseDown(e);
        }

        public override void MouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.MouseUp(e);
        }
    }
}
