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
                // We can get some info from %LOCALAPPDATA%\openvr\openvrpaths.vrpaths
                string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
                if (File.Exists(openVrPath))
                {
                    string openvrJsonString = "unknown";
                    dynamic openvrPaths = "unknown";
                    string steamVrPath = "unknown";

                    try
                    {
                        // %LOCALAPPDATA%\openvr\openvrpaths.vrpaths holds a bunch of info!
                        // We're just interested in the runtime location though. (Which is probably SteamVR)
                        JavaScriptSerializer jss = new JavaScriptSerializer();
                        openvrJsonString = File.ReadAllText(openVrPath);
                        openvrPaths = jss.DeserializeObject(openvrJsonString);
                        steamVrPath = openvrPaths["runtime"][0].ToString();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"Corrupt OpenVR Paths file... (Has SteamVR been run once?)\n\nInfo\n\topenvrJsonString: {openvrJsonString}\n\topenvrPaths: {openvrPaths}\n\tsteamVrPath: {steamVrPath}\nMessage: {e}");
                        return;
                    }
                    
                    // Just make sure the vrstartup executable actually exists...
                    string vrStartupPath = Path.Combine(steamVrPath, @"bin\win64\vrstartup.exe");
                    if (!File.Exists(vrStartupPath))
                    {
                        MessageBox.Show($"SteamVR does not exist in installation directory...\n\nInfo:\n\tsteamVrPath: {steamVrPath}\n\tvrStartupPath: {vrStartupPath}");
                        return;
                    }

                    // Start SteamVR, by the time the startup process exits, the vr server should exist...
                    Process vrStartupProcess = Process.Start(vrStartupPath);
                    vrStartupProcess.WaitForExit();

                    // Get all the processes named vrserver.exe
                    string vrServerPath = Path.Combine(steamVrPath, @"bin\win64\vrserver.exe");
                    Process[] vrServerProcesses = Process.GetProcessesByName("vrserver");

                    // Unfortunately, I don't see .Find in my intellesense, let's loop instead
                    foreach (Process vrServerProcess in vrServerProcesses)
                    {
                        // Check the process path to make sure it's SteamVR
                        if (vrServerProcess.MainModule.FileName != vrServerPath)
                        {
                            if (vrServerProcess == vrServerProcesses[vrServerProcesses.Length - 1])
                                // Huh, the VR Server does not exist...
                                MessageBox.Show($"VR Server not found... (Did SteamVR crash?)\n\nInfo:\n\tsteamVrPath: {steamVrPath}\n\tvrServerPath: {vrServerPath}\n\tvrServerProcesses.Length: {vrServerProcesses.Length}");

                            continue;
                        }
                        vrServerProcess.WaitForExit();

                        // Cool, now we know that SteamVR has shut down...
                        // Time to find the OVR Server process!
                        string oculusBase = Environment.GetEnvironmentVariable("OculusBase");
                        if (string.IsNullOrEmpty(oculusBase))
                        {
                            MessageBox.Show($"Oculus installation not found... (Try re-installing?)\n\nInfo: {oculusBase}");
                            return;
                        }

                        string ovrServerPath = Path.Combine(oculusBase, @"Support\oculus-runtime\OVRServer_x64.exe");
                        Process[] ovrServerProcesses = Process.GetProcessesByName("OVRServer_x64");

                        // No-one would ever use the name "OVRServer_x64"
                        // But let's just be safe
                        foreach (Process ovrServerProcess in ovrServerProcesses)
                        {
                            if (ovrServerProcess.MainModule.FileName != ovrServerPath)
                            {
                                if (ovrServerProcess == ovrServerProcesses[vrServerProcesses.Length - 1])
                                    // Huh, the VR Server does not exist...
                                    MessageBox.Show($"Oculus runtime not found... (Try re-installing?)\n\nInfo:\n\toculusBase: {oculusBase}\n\tovrServerPath: {ovrServerPath}\n\tovrServerProcesses.Length: {ovrServerProcesses.Length}");

                                continue;
                            }

                            // Kill it in order to end (Air)Link
                            ovrServerProcess.Kill();
                            ovrServerProcess.WaitForExit();
                        }
                    }
                }
                else
                    MessageBox.Show($"OpenVR Paths file not found... (Has SteamVR been run once?)\n\nInfo:\n\topenVrPath: {openVrPath}");
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find/start SteamVR...\n\nMessage: {e}");
            }
        }
    }
}
