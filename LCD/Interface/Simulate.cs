using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LCD.Components;
using System.Threading;

namespace LCD.Interface
{
    public class Simulate
    {
        public Circuit c;
        public CircuitView cw = null;
        public bool isRunning;

        public Simulate()
        {
            isRunning = false;
        }
        public Simulate(Circuit c)
            : this()
        {
            this.c = c;
        }

        public Simulate(Circuit c,CircuitView cw)
            : this(c)
        {
            this.cw = cw;
        }

        public Simulate(CircuitView cw)
            : this(cw.circuit, cw) { }

        public void Sim()
        {
            while (isRunning)
                lock (cw)
                {
                    c.Simulate();
                    //c.Simulate();

                    //if (cw != null)
                        cw.RedrawGates();
                    Thread.Sleep(100);
                }
        }

        public void start()
        {
            isRunning = true;
            Thread t = new Thread(new ThreadStart(Sim));
            t.Start();
            if (cw != null)
                cw.Simulating = true;
        }

        public void stop()
        {
            isRunning = false;
            if (cw != null)
                cw.Simulating = false;
        }
    }
}
