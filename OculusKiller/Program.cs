using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace OculusKiller
{
    public class Program
    {
        string openvrJsonString;
        dynamic openvrPaths;
        string steamVrPath;
        string vrStartupPath;
        string vrServerPath;
        string oculusBase;
        string ovrServerPath;

        // This should never be called before UpdateVRPaths
        Boolean isVrServer(Process vrServerProcess)
        {
            return vrServerProcess.MainModule.FileName != vrServerPath;
        }

        // Same as isVrServer, should never be called before UpdateVRPaths
        Boolean isOVrServer(Process ovrServerProcess)
        {
            return ovrServerProcess.MainModule.FileName != ovrServerPath;
        }

        Boolean UpdateVRPaths()
        {
            // We can get some usefull info from %LOCALAPPDATA%\openvr\openvrpaths.vrpaths
            string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
            if (!File.Exists(openVrPath))
            {
                MessageBox.Show($"OpenVR Paths file not found... (Has SteamVR been run once?)\n\nInfo:\n\topenVrPath: {openVrPath}");
                return false;
            }

            try
            {
                // %LOCALAPPDATA%\openvr\openvrpaths.vrpaths holds a bunch of info!
                // We're just interested in the runtime location though. (Which is probably SteamVR)
                // The VR Paths file is stored in JSON, so we need to deserialize that
                JavaScriptSerializer jss = new JavaScriptSerializer();
                openvrJsonString = File.ReadAllText(openVrPath);
                openvrPaths = jss.DeserializeObject(openvrJsonString);

                // Just update some path variables
                steamVrPath = openvrPaths["runtime"][0].ToString();
                vrStartupPath = Path.Combine(steamVrPath, @"bin\win64\vrstartup.exe");
                vrServerPath = Path.Combine(steamVrPath, @"bin\win64\vrserver.exe");

                // Update some Oculus paths
                oculusBase = Environment.GetEnvironmentVariable("OculusBase");
                ovrServerPath = Path.Combine(oculusBase, @"Support\oculus-runtime\OVRServer_x64.exe");
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Corrupt OpenVR Paths file... (Has SteamVR been run once?)\n\nInfo\n\topenvrJsonString: {openvrJsonString}\n\topenvrPaths: {openvrPaths}\n\tsteamVrPath: {steamVrPath}\nMessage: {e}");
                return false;
            }
        }

        public static void Main()
        {
            try
            {
                // Try to update our variables! We already recieve an error if it fails
                if (!UpdateVRPaths()) return;

                // Do some existence checking...
                if (!File.Exists(vrStartupPath))
                {
                    MessageBox.Show($"SteamVR does not exist in installation directory...\n\nInfo:\n\tsteamVrPath: {steamVrPath}\n\tvrStartupPath: {vrStartupPath}");
                    return;
                }

                // Start SteamVR, by the time the startup process exits, the vr server should exist...
                // We can look the vrserver process, then attach to that to figure out when SteamVR closes!
                Process vrStartupProcess = Process.Start(vrStartupPath);
                vrStartupProcess.WaitForExit();
                Process[] vrServerProcesses = Process.GetProcessesByName("vrserver");

                Process vrServerProcess = Array.Find(vrServerProcesses, isVrServer);
                if (!vrServerProcess)
                {
                    MessageBox.Show($"VR Server not found... (Did SteamVR crash?)\n\nInfo:\n\tsteamVrPath: {steamVrPath}\n\tvrServerPath: {vrServerPath}\n\tvrServerProcesses.Length: {vrServerProcesses.Length}");
                    return;
                }
                vrServerProcess.WaitForExit();

                // Once we reach here, SteamVR has shut down... (Specifically, the VR Server)
                // Now we just find the OVR Server! (Perhaps it might be more efficient to do this before waiting for the vrserver...)
                if (string.IsNullOrEmpty(oculusBase))
                {
                    MessageBox.Show($"Oculus installation not found... (Try re-installing?)\n\nInfo: {oculusBase}");
                    return;
                }

                // Find the OVR Server (So we can kill it)
                // No-one would ever use the name "OVRServer_x64" but let's just be safe...
                Process[] ovrServerProcesses = Process.GetProcessesByName("OVRServer_x64");
                Process ovrServerProcess = Array.Find(ovrServerProcesses, isOVrServer);
                if (!ovrServerProcess)
                {
                    MessageBox.Show($"Oculus runtime not found... (Try re-installing?)\n\nInfo:\n\toculusBase: {oculusBase}\n\tovrServerPath: {ovrServerPath}\n\tovrServerProcesses.Length: {ovrServerProcesses.Length}");
                    return;
                }

                // Kill it in order to end (Air)Link, it doesn't even give the user an error, nice!
                ovrServerProcess.Kill();
                ovrServerProcess.WaitForExit();
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find/start SteamVR...\n\nMessage: {e}");
            }
        }
    }
}
