using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Linq;
using System.Threading;

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

                        // At this point, vrstartup.exe will launch vrmonitor.exe.
                        var sw = Stopwatch.StartNew();
                        var vrMonitorProcesses = Process.GetProcessesByName("vrmonitor");
                        while (vrMonitorProcesses.Length == 0 && !vrStartupProcess.HasExited && sw.Elapsed.Seconds < 10)
                        {
                            Thread.Sleep(1000);
                            vrMonitorProcesses = Process.GetProcessesByName("vrmonitor");
                        }

                        // This is the one we actually need to wait for to exit when the user clicks "EXIT VR" in the SteamVR dashboard.
                        if (vrMonitorProcesses.Length > 0)
                        {
                            // While we're waiting for the user to exit VR from the SteamVR dashboard,
                            // we need to watch for the Oculus client being closed or the service being stopped.
                            // Otherwise, SteamVR will spin and hang when it gets detached from the service.
                            var oculusClientProcesses = Process.GetProcessesByName("OculusClient");
                            while (!vrMonitorProcesses.All(p => p.HasExited))
                            {
                                if (oculusClientProcesses.All(p => p.HasExited))
                                {
                                    //new Thread(() => MessageBox.Show("Oculus service or client has exited before SteamVR.  Exiting SteamVR automatically...")).Start();
                                    foreach (var process in vrMonitorProcesses)
                                    {
                                        process.Kill();
                                        process.WaitForExit();
                                    }
                                }

                                Thread.Sleep(1000);
                            }


                            // VRMonitor has exited, so kill the OVRServer process (which might end in _x86 or _x64) so it doesn't restart us when we exit.
                            foreach (var process in Process.GetProcesses().Where(p => p.ProcessName.StartsWith("OVRServer")))
                            {
                                process.Kill();
                                process.WaitForExit();
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
