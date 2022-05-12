using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                Process vrStartupProcess = Process.Start("steam://run/250820");
                vrStartupProcess.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR! (Did you install it and run it once?)\n\nMessage: {ex}");
            }
        }
    }
}
