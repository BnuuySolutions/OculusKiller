using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                RegistryKey steamVrKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 250820");
                if (steamVrKey != null)
                {
                    string installLocation = steamVrKey.GetValue("InstallLocation").ToString();
                    string vrStartupPath = Path.Combine(installLocation, @"bin\win64\vrstartup.exe");

                    if (File.Exists(vrStartupPath))
                    {
                        Process vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                        MessageBox.Show("Unable to find vrstartup executable within SteamVR installation directory.");
                }
                else
                    MessageBox.Show("Couldn't find SteamVR! (Did you install it and run it once?)");
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR! (Did you install it and run it once?)\n\nMessage: {e}");
            }
        }
    }
}
