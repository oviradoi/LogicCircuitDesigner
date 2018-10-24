using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace LCD.Components.Gates
{
    [Serializable]
    
    public abstract class Gate
    {
        public Point Location;
        [Browsable(false)]
        public Size Size { get; set; }
        [Browsable(true), Description("The angle of this gate")]
        public float Angle { get; set; }
        [Browsable(true), Description("Describe what is the purpose of this gate")]
        public String Description { get; set; }
        [Browsable(false)]
        public bool Selected { get; set; }

        public virtual void Simulate() { }
        public virtual void Draw(Graphics g) { }
        public virtual void Reset() { }
        public virtual void MouseDown(MouseEventArgs e) { }
        public virtual void MouseUp(MouseEventArgs e) { }

        public Rectangle GetRectangle()
        {
            Rectangle rectangle = Rectangle.Empty;
            if (Angle > 0 && Angle <= 90)
            {
                rectangle.X = Location.X - Size.Height;
                rectangle.Y = Location.Y;
                rectangle.Width = Size.Height;
                rectangle.Height = Size.Width;
            }

            if (Angle > 90 && Angle <= 180)
            {
                rectangle.X = Location.X - Size.Width;
                rectangle.Y = Location.Y - Size.Height;
                rectangle.Width = Size.Width;
                rectangle.Height = Size.Height;
            }

            if (Angle > 180 && Angle <= 270)
            {
                rectangle.X = Location.X;
                rectangle.Y = Location.Y - Size.Width;
                rectangle.Width = Size.Height;
                rectangle.Height = Size.Width;
            }

            if (Angle > 270 && Angle <= 360 || Angle == 0)
            {
                rectangle.X = Location.X;
                rectangle.Y = Location.Y;
                rectangle.Width = Size.Width;
                rectangle.Height = Size.Height;
            }

            return rectangle;
        }

        public virtual Dot DotOn(Point p)
        {
            return null;
        }
        public override string ToString()
        {
            return "Gate (" + Location.X + "," + Location.Y + ")";
        }
    }
}
