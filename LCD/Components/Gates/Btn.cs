using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    public class Btn : Gate
    {
        private Dot output;

        [Browsable(true),Description("The type of this button")]
        public ButtonTypes ButtonType { get; set; }

        private int ClickDownCount;
        private bool mouseDown;

        public override void Draw(System.Drawing.Graphics g)
        {
            int x, y, w, h;
            w = 30; h = 30;
            x = Location.X; y=Location.Y;
            Size = new Size(w, h);
            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, w, h));
            g.FillEllipse(output.Value == true ? Brushes.LimeGreen : Brushes.Red, new Rectangle(2, 2, w - 4, h - 4));
            g.DrawLine(Pens.Black, new Point(w, h / 2), new Point(w + 10, h / 2));
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

        public override void MouseDown(MouseEventArgs e)
        {
            if (ButtonType == ButtonTypes.PushButton)
            {
                ClickDownCount = 2;
                output.Value = true;
                mouseDown = true;
            }
            else
            {
                output.Value = !output.Value;
            }
        }

        public override void Simulate()
        {
            if(ButtonType==ButtonTypes.PushButton)
            {
                if (ClickDownCount > 0)
                    ClickDownCount--;
                if (ClickDownCount == 0 && !mouseDown)
                    output.Value = false;
            }
        }

        public override void MouseUp(MouseEventArgs e)
        {
            mouseDown = false;
        }

        public Btn(Point loc)
        {
            Location = loc;
            output = new Dot(new Point(40,15),this);
            ButtonType = ButtonTypes.ToggleButton;
        }

        public override Dot DotOn(Point p)
        {
            if ((Math.Abs(p.X - (Size.Width + 10)) <= Settings.Default.DotRadius) && (Math.Abs(p.Y - (Size.Height / 2)) <= Settings.Default.DotRadius))
                return output;
            return null;
        }

        public void SetValue(bool val)
        {
            output.Value=val;
        }

        public override string ToString()
        {
            return "Btn (" + Location.X + "," + Location.Y + ")";
        }
    }

    public enum ButtonTypes
    {
        ToggleButton = 1,
        PushButton = 2
    }
}
