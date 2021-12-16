using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null).ToString();
                if (!string.IsNullOrEmpty(steamPath))
                {
                    steamPath = Path.GetFullPath(steamPath);

                    // Going to assume SteamVR is in the default steamapps path, this could be bad.
                    var binaryPath = Path.Combine(steamPath, @"steamapps\common\SteamVR\bin\win64");
                    var vrStartupPath = Path.Combine(binaryPath, "vrstartup.exe");
                    if (Directory.Exists(binaryPath) && File.Exists(vrStartupPath))
                    {
                        var vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                        MessageBox.Show("Couldn't find SteamVR! (Did you install it and run it once?)");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR! (Did you install it and run it once?)\n\nMessage: {ex}");
            }
        }
    }
}
