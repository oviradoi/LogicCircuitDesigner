using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Runtime.Serialization;
using Settings = LCD.Properties.Settings;

namespace LCD.Components.Gates
{
    [Serializable]
    class Module : Gate
    {
        private List<Dot> inputs { get; set; }
        private List<Dot> outputs { get; set; }
        private string fileName { get; set; }
        private Circuit circuit { get; set; }

        private static readonly Font textFont = new Font("Arial", 8, FontStyle.Bold);

        public override void Simulate()
        {
            int idx = 0;
            foreach (Dot d in inputs)
            {
                if (d.w != null)
                {
                    d.Value = d.w.GetOtherDot(d).Value;
                    Btn b = circuit.GetButton(idx);
                    b.SetValue(d.Value);
                }
                idx++;
            }
            circuit.Simulate();
            idx = 0;
            foreach (Dot d in outputs)
            {
                Led l = circuit.GetLed(idx);
                d.Value=l.GetValue();
                idx++;
            }
        }

        public override void Draw(Graphics g)
        {
            string moduleName = null;
            if (!string.IsNullOrEmpty(fileName))
            {
                moduleName = Path.GetFileNameWithoutExtension(fileName);
            }

            int w, h, x, y;
            x = Location.X; y = Location.Y;
            int n = inputs.Capacity>outputs.Capacity?inputs.Capacity:outputs.Capacity;
            h = 6 * (2 * n + 1);
            SizeF textSize = g.MeasureString(moduleName, textFont);
            w = !string.IsNullOrEmpty(moduleName) ? (int) textSize.Width + 10 : 50;
            Size = new Size(w, h);

            for (int i = 0; i < outputs.Count; i++)
            {
                outputs[i].Location = new Point(w, 2 * (i + 1) * 6 - 3);
            }

            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            g.FillRectangle(Brushes.Gray, new Rectangle(0, 0, w, h));
            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, w, h));

            if (!string.IsNullOrEmpty(moduleName))
            {
                StringFormat sf = new StringFormat();
                sf.LineAlignment = StringAlignment.Center;
                sf.Alignment = StringAlignment.Center;
                g.DrawString(moduleName, textFont, Brushes.White, new RectangleF(0, 0, w, h), sf);
            }

            foreach (Dot d in inputs)
                d.Draw(g);
            foreach (Dot d in outputs)
                d.Draw(g);
            if (Selected)
                g.DrawRectangle(Pens.Blue, new Rectangle(0,0,w,h));

            g.RotateTransform(-Angle);
            g.TranslateTransform(-x, -y);
        }

        private Module(string filename)
        {
            this.fileName = filename;

            LoadModule();

            inputs = new List<Dot>(circuit.CountButtons());
            outputs = new List<Dot>(circuit.CountLeds());
            for (int i = 0; i < inputs.Capacity; i++)
            {
                inputs.Add(new Dot(new Point(0, 2 * (i + 1) * 6 - 3),this));

                inputs[i].Description=circuit.GetButton(i).Description;
            }
            for (int i = 0; i < outputs.Capacity; i++)
            {
                outputs.Add(new Dot(new Point(50, 2 * (i + 1) * 6 - 3), this));

                outputs[i].Description = circuit.GetLed(i).Description;
            }
        }

        private void LoadModule()
        {
            if (String.IsNullOrEmpty(fileName)) return;
            FileStream fs = new FileStream(fileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            circuit = (Circuit)bf.Deserialize(fs);
            fs.Close();
            fs.Dispose();
        }

        public Module(string filename, Point loc)
            : this(filename)
        {
            Location = loc;
        }

        public override Dot DotOn(Point p)
        {
            for (int i = 1; i <= inputs.Capacity; i++)
                if (Math.Abs(p.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - 2 * i * 6 + 3) <= Settings.Default.DotRadius)
                    return inputs[i - 1];
            for (int i = 1; i <= outputs.Capacity; i++)
                if (Math.Abs(Size.Width - p.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - 2 * i * 6 + 3) <= Settings.Default.DotRadius)
                    return outputs[i - 1];
            
            return null;
        }

        public override string ToString()
        {
            return "Module (" + Location.X + "," + Location.Y + ")";
        }

    }
}
