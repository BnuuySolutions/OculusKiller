using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        // yes, i know, default install location of Oculus, i'm sorry :(
        const string locationtxt = "C:\\Program Files\\Oculus\\Support\\oculus-dash\\dash\\bin\\location.txt";

        public static void Main()
        {
            try
            {
                string installLocation="";
                
                if (File.Exists(locationtxt))
                {
                    installLocation = File.ReadAllText(locationtxt);
                    if (installLocation[installLocation.Length - 1] != '\\')
                    {
                        installLocation += '\\';
                    }
                }
                else
                {
                    RegistryKey steamVrKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 250820");
                    if (steamVrKey != null)
                    {
                        installLocation = steamVrKey.GetValue("InstallLocation").ToString();
                    }
                    else
                    {
                        MessageBox.Show("Couldn't find SteamVR! (Did you install it and run it once?)");
                    }
                }
                if (installLocation != "")
                {
                    string vrStartupPath = Path.Combine(installLocation, @"bin\win64\vrstartup.exe");
                    if (File.Exists(vrStartupPath))
                    {
                        Process vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                        MessageBox.Show("Unable to find vrstartup executable within SteamVR installation directory.");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR! (Did you install it and run it once?)\n\nMessage: {e}");
            }
        }
    }
}
