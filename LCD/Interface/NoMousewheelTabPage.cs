using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LCD.Interface
{
    public class NoMousewheelTabPage : TabPage
    {
        private const int WM_MOUSEWHEEL = 0x20a;

        public NoMousewheelTabPage(string title) : base(title)
        {
        }

        protected override void WndProc(ref Message m)
        {
            // ignore WM_MOUSEWHEEL events
            if (m.Msg == WM_MOUSEWHEEL)
            {
                return;
            }

            base.WndProc(ref m);
        }
    }
}
