using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace OculusKiller
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                RegistryKey steamKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam");
                if (steamKey == null)
                {
                    MessageBox.Show("Steam is not installed!");
                    return;
                }

                string steamLocation = steamKey.GetValue("SteamPath").ToString();
                string libraryConfigLocation = Path.Combine(steamLocation, @"steamapps\libraryfolders.vdf");
                if (!File.Exists(libraryConfigLocation))
                {
                    MessageBox.Show("Steam library config does not exist!");
                }

                VProperty libraryConfig = VdfConvert.Deserialize(File.ReadAllText(libraryConfigLocation));
                foreach (VProperty child in libraryConfig.Value.Children())
                {
                    if (child.Value["apps"]["250820"] != null)
                    {
                        string steamVrLibPath = child.Value["path"].ToString();
                        string vrStartupPath = Path.Combine(steamVrLibPath, @"steamapps\common\SteamVR\bin\win64\vrstartup.exe");

                        if (File.Exists(vrStartupPath))
                        {
                            Process vrStartupProcess = Process.Start(vrStartupPath);
                            vrStartupProcess.WaitForExit();
                        }
                        else
                            MessageBox.Show("SteamVR does not exist in installation directory.");
                        return;
                    }
                }
                MessageBox.Show("SteamVR installation not found in library.");
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR!\n\nMessage: {e}");
            }
        }
    }
}
