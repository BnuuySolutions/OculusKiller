using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace OculusKiller
{
    public class Program
    {
        static Oculus oculus = new Oculus();
        static SteamVR steamVR = new SteamVR();

        static bool UpdateVars()
        {
            return oculus.Update() && steamVR.Update();
        }

        static void Main()
        {
            try
            {
                // Try to update our variables, We don't have to worry about error messages!
                // The update functions should also verify that everything is correct!
                if (!UpdateVars()) return;
                
                // Start and find the VR Server, then wait for it to exit!
                Process.Start(steamVR.startupPath).WaitForExit();

                long loopStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                while (true) {
                    // Make sure this doesn't go infinitely... Let's give it, 10 seconds!
                    if (DateTimeOffset.Now.ToUnixTimeSeconds() - loopStartTime >= 10)
                    {
                        MessageBox.Show("SteamVR VR server not found... (Did SteamVR crash?)");
                        return;
                    }

                    // Don't give the user an error if the process isn't found, it happens often...
                    Process vrServerProcess = Array.Find(Process.GetProcessesByName("vrserver"), steamVR.IsServer);
                    if (vrServerProcess == null) continue;
                    vrServerProcess.WaitForExit();

                    // Find the OVR Server (So we can kill it)
                    // No-one would ever use the name "OVRServer_x64" but let's just be safe...
                    Process ovrServerProcess = Array.Find(Process.GetProcessesByName("OVRServer_x64"), oculus.IsServer);
                    if (ovrServerProcess == null)
                    {
                        MessageBox.Show("Oculus runtime not found...");
                        return;
                    }

                    // Kill it in order to stop Link, it doesn't even give the user an error, nice!
                    ovrServerProcess.Kill();
                    ovrServerProcess.WaitForExit();
                    break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"An exception occured while attempting to find/start SteamVR...\n\nMessage: {e}");
            }
        }
    }
}
