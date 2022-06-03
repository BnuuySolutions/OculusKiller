using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace OculusKiller
{
    class SteamVR
    {
        public string location;
        public string startupPath;
        public string serverPath;

        public bool Update()
        {
            // %LOCALAPPDATA%\openvr\openvrpaths.vrpaths holds a bunch of info!
            // We're just interested in the runtime location though. (Which is probably SteamVR)
            string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
            if (!File.Exists(openVrPath))
            {
                MessageBox.Show("OpenVR Paths file not found... (Has SteamVR been run once?)");
                return false;
            }

            try
            {
                // The VR Paths file is stored in JSON, so we need to deserialize that
                JavaScriptSerializer jss = new JavaScriptSerializer();
                string openvrJsonString = File.ReadAllText(openVrPath);
                dynamic openvrPaths = jss.DeserializeObject(openvrJsonString);

                // We should only need the location/runtime variable, the rest should be derivative
                location = openvrPaths["runtime"][0].ToString();
                startupPath = Path.Combine(location, @"bin\win64\vrstartup.exe");
                serverPath = Path.Combine(location, @"bin\win64\vrserver.exe");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Corrupt OpenVR Paths file found... (Has SteamVR been run once?)\n\nMessage: {e}");
                return false;
            }

            // Now we just verify that everything exists!
            if (!File.Exists(location))
            {
                MessageBox.Show("SteamVR installation directory does not exist... (Has SteamVR been run once?)");
                return false;
            }
            if (!File.Exists(startupPath))
            {
                MessageBox.Show("SteamVR startup executable does not exist... (Has SteamVR been run once?)");
                return false;
            }
            if (!File.Exists(serverPath))
            {
                MessageBox.Show("SteamVR server executable does not exist... (Has SteamVR been run once?)");
                return false;
            }

            return true;
        }

        // We shouldn't ever need to look for the startup process...
        // Just putting it here for consistency!
        public bool IsStartup(Process startupProcess)
        {
            return startupProcess.MainModule.FileName == serverPath;
        }

        public bool IsServer(Process serverProcess)
        {
            return serverProcess.MainModule.FileName == serverPath;
        }
    }
}
