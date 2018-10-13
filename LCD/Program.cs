using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using LCD.Interface;
using Microsoft.VisualBasic.ApplicationServices;

namespace LCD
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Contains("/associate"))
            {
                if (VistaSecurity.IsAdmin())
                {
                    LCD_Settings.Associate();
                }
                Application.Exit();
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                LCDProgram program = new LCDProgram();
                program.Run(args);
            }
        }

        class LCDProgram : WindowsFormsApplicationBase
        {
            public LCDProgram()
            {
                this.IsSingleInstance = true;
                this.ShutdownStyle = ShutdownMode.AfterMainFormCloses;
            }

            protected override void OnCreateMainForm()
            {
                this.MainForm = new Interface.LCD();
            }

            protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
            {
                ((Interface.LCD)MainForm).FileOpen(eventArgs.CommandLine.ToArray());
            }
        }
    }
}
