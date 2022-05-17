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
                    var jss = new JavaScriptSerializer();
                    string jsonString = File.ReadAllText(openVrPath);
                    dynamic steamVrPath = jss.DeserializeObject(jsonString);

                    string vrStartupPath = Path.Combine(steamVrPath["runtime"][0].ToString(), @"bin\win64\vrstartup.exe");
                    if (File.Exists(vrStartupPath))
                    {
                        Process vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                        MessageBox.Show("SteamVR does not exist in installation directory.");
                }
                else
                    MessageBox.Show("Could not find openvr config file within LocalAppdata.");
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find SteamVR!\n\nMessage: {e}");
            }
        }
    }
}
