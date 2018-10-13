using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;
using System.Runtime.Serialization;
//using LCD.Properties.
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
            int w, h, x, y;
            x = Location.X; y = Location.Y;
            int n = inputs.Capacity>outputs.Capacity?inputs.Capacity:outputs.Capacity;
            w = 50; h = 6 * (2 * n + 1);
            Size = new Size(w, h);

            g.TranslateTransform(x, y);
            g.RotateTransform(Angle);

            g.FillRectangle(Brushes.Gray, new Rectangle(0, 0, w, h));
            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, w, h));

            if (fileName != "")
            {
                int ind = fileName.LastIndexOf('\\');
                String num = fileName.Remove(0, ind + 1);
                ind = num.LastIndexOf('.');
                num = num.Remove(ind);
                g.DrawString(num, new Font("Arial", 8, FontStyle.Bold), new SolidBrush(Color.White)
                , new PointF((w - num.Length * 8) / 2, h / 2 - 8));
            }

            /*for (int i = 1; i <= n; i++)
                g.DrawLine(Pens.Black, new Point(0, 2 * i * 6 - 3), new Point(10, 2 * i * 6 - 3));

            g.DrawLine(Pens.Black, new Point(w - 21, h / 2 - 0), new Point(w - 1, h / 2 - 0));

            g.DrawLine(Pens.Black, new Point(10, 0), new Point(10, h - 1));
            g.DrawArc(Pens.Black, new Rectangle(-7, 0, w - 15, h - 1), -90, 180);*/

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
                if (Math.Abs(50 - p.X) <= Settings.Default.DotRadius && Math.Abs(p.Y - 2 * i * 6 + 3) <= Settings.Default.DotRadius)
                    return outputs[i - 1];
            
            return null;
        }

        public override string ToString()
        {
            return "Module (" + Location.X + "," + Location.Y + ")";
        }

    }
}
