using OculusKillerInstaller;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            //CHECK IF APP IS ALREADY RUNNING (ONLY ONE INSTANCE)
            using (var mutex = new Mutex(false, "OculusKillerInstaller"))
            {
                bool isAnotherInstanceOpen = !mutex.WaitOne(TimeSpan.Zero);
                if (isAnotherInstanceOpen)
                {
                    return;
                }

                //START APP
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
