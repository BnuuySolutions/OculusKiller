using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Linq;

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

                        // At this point, vrstartup.exe should have launched vrmonitor.exe.
                        // This is the one we actually need to wait for to exit when the user clicks "EXIT VR" in the SteamVR dashboard.
                        var vrMonitorProcesses = Process.GetProcessesByName("vrmonitor");
                        if (vrMonitorProcesses.Length > 0)
                        {
                            foreach (var vrMonitorProcess in vrMonitorProcesses)
                            {
                                vrMonitorProcess.WaitForExit();
                            }

                            // Since exiting SteamVR doesn't actually tell the OVRService to disconnect the headset,
                            // the OVRService will still immediately re-launch if we exit here.
                            // Even if we return the same exit code as the official OculusDash.exe
                            // does when the user presses Quit, which is 0xc0000005, it doesn't stop OVRServer from re-launching us.

#if false // Doesn't work without admin privileges.
                            // The only way we know of to prevent this is to stop the OVRService service or kill the OVRServer process.
                            var ovrService = new System.ServiceProcess.ServiceController("OVRService");
                            ovrService.Stop(); 
                            ovrService.WaitForStatus(System.ServiceProcess.ServiceControllerStatus.Stopped);
                            // Restart it so that it's ready for the next time the user wants to connect their headset.
                            ovrService.Start();
#endif

#if false // Thwarted by Oculus Client's "Are you sure you want to quit?" dialog.
                            // Gracefully stop the OculusClient process.
                            var oculusClientProcesses = Process.GetProcessesByName("OculusClient");
                            if (oculusClientProcesses.Length > 0)
                            {
                                foreach (var oculusClientProcess in oculusClientProcesses)
                                {
                                    oculusClientProcess.CloseMainWindow();
                                }
                                foreach (var oculusClientProcess in oculusClientProcesses)
                                {
                                    oculusClientProcess.WaitForExit();
                                }
                            }
                            else
                                MessageBox.Show("Could not find OculusClient process.");
#endif

                            // Last resort: Kill the OVRServer process (might end in _x86 or _x64).
                            foreach (var ovrServerProcess in Process.GetProcesses().Where(p => p.ProcessName.StartsWith("OVRServer")))
                            {
                                ovrServerProcess.Kill();
                            }

                            // The OVRServiceLauncher will immediately re-launch OVRServer and the OculusClient window,
                            // which is kind of annoying, but at least it will set our audio/microphone devices back to the default ones.
                            
                            // We can safely exit now, since killing the OVRServer process already disconnected the headset.
                        }
                        else
                            MessageBox.Show("Could not find vrmonitor process.");
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
