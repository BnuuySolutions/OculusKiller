using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace OculusKiller
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
                if (File.Exists(openVrPath))
                {
                    string jsonString;
                    dynamic openvrPaths;
                    string steamVrPath;

                    try
                    {
                        // %LOCALAPPDATA%\openvr\openvrpaths.vrpaths holds a bunch of info!
                        // We're just interested in the runtime location though. (Which is probably SteamVR)
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        jsonString = File.ReadAllText(openVrPath);
                        openvrPaths = jss.DeserializeObject(jsonString);
                        steamVrPath = openvrPaths["runtime"][0].ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Corrupt OpenVR Paths file... (Has SteamVR been run once?)\n\nMessage: {e}");
                        return;
                    }
                    
                    // Just make sure the vrstartup executable actually exists...
                    string vrStartupPath = Path.Combine(steamVrPath, @"bin\win64\vrstartup.exe");
                    if (!File.Exists(vrStartupPath))
                    {
                        MessageBox.Show("SteamVR does not exist in installation directory...");
                        return;
                    }

                    // Start SteamVR, by the time the startup process exits, the vr server should exist...
                    Process vrStartupProcess = Process.Start(vrStartupPath);
                    vrStartupProcess.WaitForExit();
                }
                else
                    MessageBox.Show("OpenVR Paths file not found... (Has SteamVR been run once?)");
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find/start SteamVR...\n\nMessage: {e}");
            }
        }
    }
}
