using System;
using System.Collections.Generic;
using System.Windows.Forms;
using LCD.Components;
using System.Drawing;
using LCD.Components.Gates;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Printing;
using Settings = LCD.Properties.Settings;

namespace LCD.Interface
{
    public class CircuitView : PictureBox
    {
        #region Fields

        public Circuit circuit { get; set; }
        private bool isMouseDownGate = false;
        private bool isMouseDownDot = false;
        public bool Simulating { get; set; }
        private Dot selectedDot = null;
        private WirePoint selectedWP = null;

        private Point MouseDownPosition;
        private Point MouseCurrentPosition;
        private bool saved;
        private PrintDocument printDoc = new PrintDocument();
        private Gate floatingGate = null;

        public bool Saved
        {
            get
            {
                return saved;
            }
            protected set
            {
                saved = value;
                circuit.SaveState();
            }
        }

        public string FileName { get; set; }
        private ToolTip toolTip = new ToolTip();
        private Gate lastToolTippedGate = null;
        private Dot lastToolTippedDot = null;
        private Rectangle selectionRectangle = Rectangle.Empty;

        #endregion

        public event GateSelectedEvent OnGateSelected;

        public delegate void GateSelectedEvent(Gate gate);
        public CircuitView()
        {
            circuit = new Circuit();
            Initialize();
            Saved = true;
        }

        public CircuitView(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            circuit = (Circuit)bf.Deserialize(fs);
            fs.Close();
            fs.Dispose();
            this.FileName = FileName;

            Initialize();
            Saved = true;
            ResizeAtLoad();
        }

        public void Reset()
        {
            foreach (Gate g in circuit.Gates)
            {
                g.Reset();
            }
        }

        private void Initialize()
        {
            Image = new Bitmap(Size.Width, Size.Height);
            SizeChanged += new EventHandler(CircuitView_SizeChanged);
            MouseDown += new MouseEventHandler(CircuitView_MouseDown);
            MouseUp += new MouseEventHandler(CircuitView_MouseUp);
            MouseMove += new MouseEventHandler(CircuitView_MouseMove);
            MouseWheel += new MouseEventHandler(CircuitView_MouseWheel);
            MouseEnter += new EventHandler(CircuitView_MouseEnter);
            KeyDown += new KeyEventHandler(CircuitView_KeyDown);
            printDoc.PrintPage+= new PrintPageEventHandler(printDoc_PrintPage);
        }

        public void RedrawGates()
        {
            RedrawGates(null);
        }

        private void RedrawGates(Graphics graph)
        {
            Graphics gr;
            if (graph == null)
            {
                gr = Graphics.FromImage(Image);
            }
            else
            {
                gr = graph;
            }
            gr.Clear(Settings.Default.CircuitBackColor);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (Gate g in circuit.Gates)
            {
                g.Draw(gr);
            }

            foreach (Wire w in circuit.Wires)
            {
                Point a, b;
                a = new Point(w.src.Location.X + w.src.Parent.Location.X, w.src.Location.Y + w.src.Parent.Location.Y);
                b = new Point(w.dst.Location.X + w.dst.Parent.Location.X, w.dst.Location.Y + w.dst.Parent.Location.Y);
                a = RotatePoint(a, w.src.Parent.Location, w.src.Parent.Angle);
                b = RotatePoint(b, w.dst.Parent.Location, w.dst.Parent.Angle);

                Pen pen;
                if (w.dst.Value || w.src.Value)
                {
                    pen = new Pen(Settings.Default.WireOnColor);
                }
                else
                {
                    pen = new Pen(Settings.Default.WireOffColor);
                }

                pen.Width = w.Selected ? 2 : 1;
                Point lastPoint = a;
                foreach (WirePoint currentPoint in w.Points)
                {
                    gr.DrawLine(pen, lastPoint, currentPoint.Location);
                    Pen penWirePoint = new Pen(Settings.Default.WirePointColor);

                    gr.DrawEllipse(
                        penWirePoint,
                        currentPoint.X - Settings.Default.WirePointRadius / 2,
                        currentPoint.Y - Settings.Default.WirePointRadius / 2,
                        Settings.Default.WirePointRadius,
                        Settings.Default.WirePointRadius);

                    SolidBrush brushWirePoint = new SolidBrush(Settings.Default.WirePointColor);

                    gr.FillEllipse(
                        brushWirePoint,
                        currentPoint.X - Settings.Default.WirePointRadius / 2,
                        currentPoint.Y - Settings.Default.WirePointRadius / 2,
                        Settings.Default.WirePointRadius,
                        Settings.Default.WirePointRadius);

                    lastPoint = currentPoint.Location;
                }
                gr.DrawLine(pen, lastPoint, b);
            }

            if (isMouseDownDot)
            {
                Pen pen = new Pen(Settings.Default.WireOffColor);
                Point absoluteLocation = new Point(selectedDot.Location.X + selectedDot.Parent.Location.X,
                    selectedDot.Location.Y + selectedDot.Parent.Location.Y);
                gr.DrawLine(pen, absoluteLocation, MouseCurrentPosition);
            }

            if (selectionRectangle != Rectangle.Empty)
            {
                gr.DrawRectangle(Pens.Black, selectionRectangle);
            }

            if (graph == null) 
                Image = Image;
        }

        //Resizes the component after loading a circuit from file
        //Some gates may not be shown if this function is not called
        private void ResizeAtLoad()
        {

            foreach (Gate gate in circuit.Gates)
            {
                AcceptMoveAndResize(gate.Location, gate);
            }
            foreach (Wire wire in circuit.Wires)
            {
                foreach (WirePoint wirePoint in wire.Points)
                {
                    AcceptMoveAndResize(wirePoint.Location,
                        new Size(Settings.Default.WirePointRadius, Settings.Default.WirePointRadius));
                }
            }

        }

        private void ShowToolTip(String text, int seconds, Point location)
        {
            if (seconds > 0)
            {
                toolTip.Show(text, this, location.X, location.Y, seconds*1000);
            }
            
        }

        public void DumpToDisk(string FileName)
        {
            FileStream fs = new FileStream(FileName, FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, circuit);
            fs.Close();
            fs.Dispose();

            Saved = true;
        }

        #region Add Delete Move Resize

        public void AddGate(Gate g)
        {
            SelectNone();
            floatingGate = g;
            floatingGate.Selected = true;
            if (OnGateSelected != null)
                OnGateSelected(g);
            isMouseDownGate = true;
            MouseDownPosition = new Point(10, 10);

            circuit.Gates.Add(g);
            RedrawGates();

            Saved = false;
        }

        private void AddWire(Dot start, Dot dest)
        {
            Wire w = new Wire();
            w.src = dest;
            w.dst = start;
            dest.w = w;
            start.w = w;
            circuit.Wires.Add(w);

            Saved = false;
        }

        private void DeleteSelected()
        {
            if (Simulating) return;
            List<Gate> dell = new List<Gate>();
            List<Wire> delw = new List<Wire>();
            foreach (Gate g in circuit.Gates)
            {
                if (g.Selected)
                {
                    dell.Add(g);
                }
            }
            foreach (Wire w in circuit.Wires)
            {
                if (w.Selected)
                    delw.Add(w);
                else
                    foreach (Gate g in dell)
                    {
                        if (w.src.Parent == g || w.dst.Parent == g)
                            delw.Add(w);
                    }
            }
            foreach (Gate g in dell)
                circuit.Gates.Remove(g);
            foreach (Wire w in delw)
                circuit.Wires.Remove(w);

            if (dell.Count != 0 || delw.Count != 0)
            {
                Saved = false;
            }

            RedrawGates();
        }


        private bool AcceptMoveAndResize(Point newLocation, Gate gate)
        {
            if (newLocation.X < 0 || newLocation.Y < 0)
            {
                return false;
            }

            //Resize the width 
            if (newLocation.X + gate.Size.Width > this.Width)
            {
                this.Width = newLocation.X + 2 * gate.Size.Width;

            }

            //Resize the height
            if (newLocation.Y + gate.Size.Height > this.Height)
            {
                this.Height = newLocation.Y + 2 * gate.Size.Height;

            }

            return true;
        }

        private bool AcceptMoveAndResize(Point newLocation, Size size)
        {
            if (newLocation.X < 0 || newLocation.Y < 0)
            {
                return false;
            }

            //Resize the width 
            if (newLocation.X + size.Width > this.Width)
            {
                this.Width = newLocation.X + 2 * size.Width;

            }

            //Resize the height
            if (newLocation.Y + size.Height > this.Height)
            {
                this.Height = newLocation.Y + 2 * size.Height;

            }

            return true;
        }

        #endregion
        #region Keyboard events

        void CircuitView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteSelected();
        }

        void CircuitView_SizeChanged(object sender, EventArgs e)
        {
            Image = new Bitmap(Size.Width, Size.Height);
            RedrawGates();
        }

        #endregion
        #region Mouse events

        private void CircuitView_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Simulating) return;
            int WHEEL_DELTA = 120;
            foreach (Gate g in circuit.Gates)
            {
                if (g.Selected)
                {
                    if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        g.Angle -= Math.Sign(e.Delta);
                    else
                        g.Angle -= (4 * e.Delta / WHEEL_DELTA);
                }
            }
            RedrawGates();
        }

        private void CircuitView_MouseMove(object sender, MouseEventArgs e)
        {
            MouseCurrentPosition = e.Location;
            if (Simulating) return;
            if (isMouseDownGate)
            {
                foreach (Gate gate in circuit.Gates)
                {
                    if (gate.Selected == true)
                    {
                        Point newLocation = new Point(gate.Location.X + e.Location.X - MouseDownPosition.X,
                            gate.Location.Y + e.Location.Y - MouseDownPosition.Y);

                        if (AcceptMoveAndResize(newLocation, gate))
                        {
                            gate.Location = newLocation;
                        }
                    }
                }
                MouseDownPosition = e.Location;
                RedrawGates();
            }
            if (selectedWP != null)
            {
                if (AcceptMoveAndResize(
                    e.Location,
                    new Size(
                        Settings.Default.WirePointRadius,
                        Settings.Default.WirePointRadius)))
                {
                    selectedWP.Location = e.Location;
                    RedrawGates();
                }
            }

            //Verify whether the mouse is over a gate
            //and that no button was pressed

            Gate gateMouseOver = GateOn(e.Location);

            if (e.Button == MouseButtons.None &&
                gateMouseOver != null &&
                lastToolTippedGate != gateMouseOver)
            {
                String gateDescription = gateMouseOver.Description;

                if (gateDescription != null && gateDescription.Length != 0)
                {
                    ShowToolTip(
                        gateDescription,
                        Settings.Default.DescriptionToolTipTimeOut,
                        new Point(
                            gateMouseOver.Location.X + 5,
                            gateMouseOver.Location.Y - 15));

                    lastToolTippedGate = gateMouseOver;
                }
            }
            else
            {
                if (gateMouseOver == null)
                {
                    lastToolTippedGate = null;
                }
            }

            if (isMouseDownDot)
            {
                RedrawGates();
            }

            if (selectionRectangle != Rectangle.Empty)
            {
                Point start = MouseDownPosition;
                Point end = e.Location;
                selectionRectangle = new Rectangle(Math.Min(start.X, end.X),
                    Math.Min(start.Y, end.Y),
                    Math.Abs(start.X - end.X),
                    Math.Abs(start.Y - end.Y));
                RedrawGates();
            }

            //Cross Cursor Code

            Dot dotOver = DotOn(e.Location);

            if (gateMouseOver == null && dotOver == null)
            {
                lastToolTippedDot = null;
                lastToolTippedGate = null;
            }

            if (dotOver != null)
            {
                this.Cursor = Cursors.Cross;

                if (dotOver.Parent is Module &&
                    dotOver.Description != null &&
                    dotOver.Description.Length != 0)
                {
                    Gate gateParent = dotOver.Parent;

                    if (e.Button == MouseButtons.None &&
                        dotOver != lastToolTippedDot)
                    {

                        ShowToolTip(
                            dotOver.Description,
                            Settings.Default.DescriptionToolTipTimeOut,
                            new Point(
                                gateParent.Location.X + 5,
                                gateParent.Location.Y - 15));

                        lastToolTippedDot = dotOver;
                    }
                }
            }
            else
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void CircuitView_MouseUp(object sender, MouseEventArgs e)
        {
            if (Simulating)
            {
                Gate g = GateOn(e.Location);
                if (g != null)
                {
                    Point rotatedPoint = RotatePoint(e.Location, g.Location, -g.Angle);
                    MouseEventArgs eventArgs = new MouseEventArgs(
                        e.Button,
                        e.Clicks,
                        rotatedPoint.X,
                        rotatedPoint.Y,
                        e.Delta);
                    g.MouseUp(eventArgs);
                }
            }
            isMouseDownGate = false;
            if (isMouseDownDot)
            {
                Dot d = DotOn(e.Location);

                if (d != null && selectedDot != d)
                {
                    //Don't let 2 dots of the same gate connected

                    if (d.Parent != selectedDot.Parent)
                    {
                        AddWire(d, selectedDot);
                    }
                }
                isMouseDownDot = false;
                RedrawGates();
            }

            if (selectionRectangle != Rectangle.Empty)
            {
                SelectComponentsInSelectionRectangle();
                selectionRectangle = Rectangle.Empty;
                RedrawGates();
            }

            if (e.Button == MouseButtons.Middle)
            {
                Wire wire = WireOn(e.Location);
                if (wire != null)
                {
                    if (selectedWP != null)
                    {
                        wire.Points.Remove(selectedWP);
                    }
                    else
                    {
                        int idx = BeforeWirePoint(e.Location, wire);
                        wire.Points.Insert(idx, new WirePoint(e.Location));
                    }
                    RedrawGates();
                }
            }
            selectedWP = null;
        }

        private void SelectComponentsInSelectionRectangle()
        {
            List<Gate> gatesInSelectionRectangle = new List<Gate>();
            List<Wire> wiresInSelectionRectangle = new List<Wire>();

            foreach (Gate gate in circuit.Gates)
            {
                Rectangle gateRectangle = gate.GetRectangle();

                if (gateRectangle.IntersectsWith(selectionRectangle))
                {
                    if (!gatesInSelectionRectangle.Contains(gate))
                    {
                        gatesInSelectionRectangle.Add(gate);
                    }
                }
                else
                {
                    if (gatesInSelectionRectangle.Contains(gate))
                    {
                        gatesInSelectionRectangle.Remove(gate);
                    }
                }

            }

            foreach (Wire wire in circuit.Wires)
            {
                if (gatesInSelectionRectangle.Contains(wire.src.Parent) && gatesInSelectionRectangle.Contains(wire.dst.Parent))
                {
                    if (!wiresInSelectionRectangle.Contains(wire))
                    {
                        wiresInSelectionRectangle.Add(wire);
                    }
                }
            }

            SelectNone();
            foreach (Gate g in gatesInSelectionRectangle)
            {
                g.Selected = true;
            }

            foreach (Wire w in wiresInSelectionRectangle)
            {
                w.Selected = true;
            }

            if (gatesInSelectionRectangle.Count > 0)
            {
                if (OnGateSelected != null)
                {
                    OnGateSelected(gatesInSelectionRectangle[0]);
                }
            }
        }

        private void CircuitView_MouseDown(object sender, MouseEventArgs e)
        {
            Gate g = GateOn(e.Location);
            Dot d = DotOn(e.Location);
            WirePoint wp = WirePointOn(e.Location);
            Wire wire = WireOn(e.Location);
            if (Simulating)
            {
                if (g != null)
                {
                    Point rotatedPoint = RotatePoint(e.Location, g.Location, -g.Angle);
                    MouseEventArgs eventArgs = new MouseEventArgs(
                        e.Button,
                        e.Clicks,
                        rotatedPoint.X,
                        rotatedPoint.Y,
                        e.Delta);
                    g.MouseDown(eventArgs);
                }
                return;
            }
            if (d == null && g == null && wp == null && wire == null)
            {
                ModifySelection(true, true, false);
                if (OnGateSelected != null)
                    OnGateSelected(null);
                RedrawGates();
            }

            if (d != null)
            {
                Dot_MouseDown(d, e.Location);
            }
            else
            {
                if (g != null)
                {
                    Gate_MouseDown(g, e.Location);
                }
                else
                {
                    if (wp != null)
                    {
                        selectedWP = wp;
                    }
                    else
                    {
                        if (wire != null)
                        {
                            Wire_MouseDown(wire);
                        }
                        else
                        {
                            MouseDownPosition = e.Location;
                            selectionRectangle = new Rectangle(e.Location, Size.Empty);
                        }
                    }
                }
            }
        }

        void CircuitView_MouseEnter(object sender, EventArgs e)
        {
            Focus();
        }

        private void Wire_MouseDown(Wire wire)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                wire.Selected = !wire.Selected;
            }
            else
            {
                if (wire.Selected == false)
                {
                    ModifySelection(true, true, false);
                    wire.Selected = true;
                }
            }
            RedrawGates();
        }

        private void Dot_MouseDown(Dot d, Point location)
        {
            isMouseDownDot = true;
            selectedDot = d;
        }

        void Gate_MouseDown(Gate g, Point location)
        {
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                g.Selected = !g.Selected;
            }
            else
            {
                if (g.Selected == false)
                {
                    ModifySelection(true, true, false);
                    g.Selected = true;
                    if (OnGateSelected != null)
                        OnGateSelected(g);
                }
            }

            MouseDownPosition = location;
            isMouseDownGate = true;
            RedrawGates();
        }

        private Point RotatePoint(Point input, Point reference, double angle)
        {
            Point ret = new Point();
            double cos, sin;
            cos = Math.Cos(angle * Math.PI / 180);
            sin = Math.Sin(angle * Math.PI / 180);
            ret.X = (int)(cos * (input.X - reference.X) - sin * (input.Y - reference.Y) + reference.X);
            ret.Y = (int)(sin * (input.X - reference.X) + cos * (input.Y - reference.Y) + reference.Y);
            return ret;
        }

        private void ModifySelection(bool gates, bool wires, bool value)
        {
            if (gates)
                foreach (Gate gate in circuit.Gates)
                    gate.Selected = value;
            if (wires)
                foreach (Wire w in circuit.Wires)
                    w.Selected = value;
        }

        #endregion

        #region Hit tests

        private Gate GateOn(Point location)
        {
            foreach (Gate g in circuit.Gates)
            {
                Point p = RotatePoint(location, g.Location, -g.Angle);
                Rectangle r = new Rectangle(g.Location, g.Size);
                if (r.Contains(p))
                    return g;
            }
            return null;
        }

        private Dot DotOn(Point location)
        {
            Dot d;
            foreach (Gate g in circuit.Gates)
            {
                Point p = RotatePoint(location, g.Location, -g.Angle);
                d = g.DotOn(new Point(p.X - g.Location.X, p.Y - g.Location.Y));
                if (d != null)
                    return d;
            }
            return null;
        }

        private WirePoint WirePointOn(Point location)
        {
            foreach (Wire w in circuit.Wires)
            {
                foreach (WirePoint p in w.Points)
                {
                    if (p.PointOn(location))
                    {
                        return p;
                    }
                }
            }
            return null;
        }

        private Wire WireOn(Point point)
        {
            foreach (Wire w in circuit.Wires)
            {
                Point a, b;
                a = new Point(w.src.Location.X + w.src.Parent.Location.X, w.src.Location.Y + w.src.Parent.Location.Y);
                b = new Point(w.dst.Location.X + w.dst.Parent.Location.X, w.dst.Location.Y + w.dst.Parent.Location.Y);
                a = RotatePoint(a, w.src.Parent.Location, w.src.Parent.Angle);
                b = RotatePoint(b, w.dst.Parent.Location, w.dst.Parent.Angle);
                Point lastPoint = a;
                foreach (WirePoint currentPoint in w.Points)
                {
                    if (PointOnLine(lastPoint, currentPoint.Location, point))
                        return w;
                    lastPoint = currentPoint.Location;
                }
                if (PointOnLine(lastPoint, b, point))
                    return w;
            }
            return null;
        }

        private int BeforeWirePoint(Point point, Wire w)
        {
            int x = 0;
            Point a, b;
            a = new Point(w.src.Location.X + w.src.Parent.Location.X, w.src.Location.Y + w.src.Parent.Location.Y);
            b = new Point(w.dst.Location.X + w.dst.Parent.Location.X, w.dst.Location.Y + w.dst.Parent.Location.Y);
            a = RotatePoint(a, w.src.Parent.Location, w.src.Parent.Angle);
            b = RotatePoint(b, w.dst.Parent.Location, w.dst.Parent.Angle);
            Point lastPoint = a;
            foreach (WirePoint currentPoint in w.Points)
            {
                if (PointOnLine(lastPoint, currentPoint.Location, point))
                    return x;
                x++;
                lastPoint = currentPoint.Location;
            }
            if (PointOnLine(lastPoint, b, point))
                return x;
            return 0;
        }

        private bool PointOnLine(Point ls, Point le, Point p)
        {
            double numarator1, numitor1;
            double numarator2, numitor2;
            numarator1 = p.X - ls.X;
            numitor1 = le.X - ls.X;
            numarator2 = p.Y - ls.Y;
            numitor2 = le.Y - ls.Y;
            double fr1, fr2;
            if (numitor1 == 0)
                fr1 = 0;
            else
                fr1 = numarator1 / numitor1;
            if (numitor2 == 0)
                fr2 = 0;
            else
                fr2 = numarator2 / numitor2;

            int minx, maxx, miny, maxy;
            minx = ls.X < le.X ? ls.X : le.X;
            maxx = ls.X > le.X ? ls.X : le.X;
            miny = ls.Y < le.Y ? ls.Y : le.Y;
            maxy = ls.Y > le.Y ? ls.Y : le.Y;
            if (minx == maxx)
            {
                minx -= 1;
                maxx += 1;
            }
            if (miny == maxy)
            {
                miny -= 1;
                maxy += 1;
            }

            if (minx <= p.X && p.X <= maxx && miny <= p.Y && p.Y <= maxy)
            {
                if (fr1 == 0 || fr2 == 0)
                    return true;
                if (Math.Abs(fr1 - fr2) < 0.03)
                    return true;
            }
            return false;
        }

        #endregion

        #region Selection Methods

        private void RemoveNonConnectedWires()
        {
            int loop = 0;

            while (loop < circuit.Wires.Count)
            {
                Wire wire = circuit.Wires[loop];

                if (!circuit.Gates.Contains(wire.src.Parent) ||
                    !circuit.Gates.Contains(wire.dst.Parent))
                {
                    circuit.Wires.Remove(wire);
                }
                else
                {
                    loop++;
                }
            }
        }

        //All selection

        public void SelectAllGates()
        {
            foreach (Gate gate in circuit.Gates)
            {
                gate.Selected = true;
            }

            RedrawGates();
        }

        public void SelectAllWires()
        {
            foreach (Wire wire in circuit.Wires)
            {
                wire.Selected = true;
            }

            RedrawGates();
        }

        public void SelectAll()
        {
            SelectAllGates();
            SelectAllWires();
        }

        //None selection

        public void SelectNoneGates()
        {
            foreach (Gate gate in circuit.Gates)
            {
                gate.Selected = false;
            }

            RedrawGates();
        }

        public void SelectNoneWires()
        {
            foreach (Wire wire in circuit.Wires)
            {
                wire.Selected = false;
            }

            RedrawGates();
        }

        public void SelectNone()
        {
            SelectNoneGates();
            SelectNoneWires();
        }

        //Invert selection

        public void InvertGatesSelection()
        {
            foreach (Gate gate in circuit.Gates)
            {
                gate.Selected = !gate.Selected;
            }

            RedrawGates();
        }

        public void InvertWiresSelection()
        {
            foreach (Wire wire in circuit.Wires)
            {
                wire.Selected = !wire.Selected;
            }

            RedrawGates();
        }

        public void InvertSelection()
        {
            InvertGatesSelection();
            InvertWiresSelection();
        }

        //Crop selection

        public void CropGatesSelection()
        {
            int loop = 0;

            while (loop < circuit.Gates.Count)
            {
                if (!circuit.Gates[loop].Selected)
                {
                    circuit.Gates.Remove(circuit.Gates[loop]);
                }
                else
                {
                    loop++;
                }
            }

            RemoveNonConnectedWires();

            RedrawGates();
        }

        public void CropWiresSelection()
        {
            int loop = 0;

            while (loop < circuit.Wires.Count)
            {
                if (!circuit.Wires[loop].Selected)
                {
                    circuit.Wires.Remove(circuit.Wires[loop]);
                }
                else
                {
                    loop++;
                }
            }

            RedrawGates();
        }

        public void CropSelection()
        {
            CropGatesSelection();
            CropWiresSelection();
        }

        //Delete selection

        public void DeleteGatesSelection()
        {
            int loop = 0;

            while (loop < circuit.Gates.Count)
            {
                if (circuit.Gates[loop].Selected)
                {
                    circuit.Gates.Remove(circuit.Gates[loop]);
                }
                else
                {
                    loop++;
                }
            }

            RemoveNonConnectedWires();

            RedrawGates();
        }

        public void DeleteWiresSelection()
        {
            int loop = 0;

            while (loop < circuit.Wires.Count)
            {
                if (circuit.Wires[loop].Selected)
                {
                    circuit.Wires.Remove(circuit.Wires[loop]);
                }
                else
                {
                    loop++;
                }
            }

            RedrawGates();
        }

        public void DeleteSelection()
        {
            DeleteGatesSelection();
            DeleteWiresSelection();
        }

        #endregion

        #region Align Methods

        protected Gate[] GetSelectedGates()
        {
            List<Gate> selectedGateList = new List<Gate>();

            foreach (Gate gate in circuit.Gates)
            {
                if (gate.Selected)
                {
                    selectedGateList.Add(gate);
                }
            }

            return selectedGateList.ToArray();
        }

        protected Wire[] GetSelectedWires()
        {
            List<Wire> selectedWireList = new List<Wire>();

            foreach (Wire wire in circuit.Wires)
            {
                if (wire.Selected)
                {
                    selectedWireList.Add(wire);
                }
            }

            return selectedWireList.ToArray();
        }

        public void AlignLeftSides()
        {
            Gate[] selectedGates = GetSelectedGates();

            int minLeft = this.Width;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                if (minLeft > selectedGates[i].Location.X)
                {
                    minLeft = selectedGates[i].Location.X;
                }
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.X = minLeft;
            }

            RedrawGates();
        }

        public void AlignVerticalCenters()
        {
            Gate[] selectedGates = GetSelectedGates();

            int sumOfYCoordonates = 0;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                sumOfYCoordonates += selectedGates[i].Location.X;
            }

            int average;

            unchecked
            {
                average = sumOfYCoordonates / selectedGates.Length;
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.X = average;
            }

            RedrawGates();
        }

        public void AlignRightSides()
        {
            Gate[] selectedGates = GetSelectedGates();

            int maxLeft = -1;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                if (maxLeft < selectedGates[i].Location.X)
                {
                    maxLeft = selectedGates[i].Location.X;
                }
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.X = maxLeft;
            }

            RedrawGates();
        }

        public void AlignTopEdges()
        {
            Gate[] selectedGates = GetSelectedGates();

            int minTop = this.Height;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                if (minTop > selectedGates[i].Location.Y)
                {
                    minTop = selectedGates[i].Location.Y;
                }
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.Y = minTop;
            }

            RedrawGates();
        }

        public void AlignHorizontalSides()
        {
            Gate[] selectedGates = GetSelectedGates();

            int sumOfYCoordonates = 0;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                sumOfYCoordonates += selectedGates[i].Location.Y;
            }

            int average;

            unchecked
            {
                average = sumOfYCoordonates / selectedGates.Length;
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.Y = average;
            }

            RedrawGates();
        }

        public void AlignBottomEdges()
        {
            Gate[] selectedGates = GetSelectedGates();

            int maxBottom = -1;

            for (int i = 0; i < selectedGates.Length; i++)
            {
                if (maxBottom < selectedGates[i].Location.Y)
                {
                    maxBottom = selectedGates[i].Location.Y;
                }
            }

            for (int i = 0; i < selectedGates.Length; i++)
            {
                selectedGates[i].Location.Y = maxBottom;
            }

            RedrawGates();
        }

        #endregion

        #region Cut Copy Paste Undo Redo

        private const String clipboardLCDFormat = "LCDCircuit";

        public void Cut()
        {
            Copy();

            DeleteSelected();
        }

        public void Copy()
        {
            Gate[] selectedGates = GetSelectedGates();
            Wire[] selectedWires = GetSelectedWires();

            Circuit circuit = new Circuit();

            circuit.Gates.AddRange(selectedGates);
            circuit.Wires.AddRange(selectedWires);

            Clipboard.SetData(clipboardLCDFormat, circuit);
        }

        public void Paste()
        {
            SelectNone();

            if (Clipboard.ContainsData(clipboardLCDFormat))
            {
                Circuit pastedCircuit = (Circuit)Clipboard.GetData(clipboardLCDFormat);

                if (pastedCircuit != null &&
                    pastedCircuit.Gates != null &&
                    pastedCircuit.Wires != null)
                {
                    circuit.Gates.AddRange(pastedCircuit.Gates);
                    circuit.Wires.AddRange(pastedCircuit.Wires);

                    if (pastedCircuit.Gates.Count != 0 ||
                        pastedCircuit.Wires.Count != 0)
                    {
                        Saved = false;
                    }
                }
            }

            RedrawGates();
        }

        public void Undo()
        {
            circuit.Undo();

            RedrawGates();
        }

        public void Redo()
        {
            circuit.Redo();

            RedrawGates();
        }

        #endregion

        #region Print

        public void Print()
        {
            PrintDialog printDialog = new PrintDialog();

            //Let it be an extended dialog
            //If it is set to false the dialog will
            //not show on some machines
            printDialog.UseEXDialog = true;
            printDialog.Document = printDoc;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDoc.Print();
            }
        }

        public void PrintPreview()
        {
            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDoc;
            if (printPreviewDialog.ShowDialog() == DialogResult.OK)
            {
                Print();
            }
        }

        private void printDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            RedrawGates(e.Graphics);
        }

        #endregion
    }
}
