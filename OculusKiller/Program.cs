using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        // Define the path for the log file
        private static string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OculusKiller", "logs.txt");

        public static async Task Main()
        {
            try
            {
                // Initialize the logging mechanism
                InitializeLogging();
                Log("Application started.");

                // Get the path for Oculus
                string oculusPath = GetOculusPath();
                if (string.IsNullOrEmpty(oculusPath))
                {
                    Log("Oculus path not found.");
                    return;
                }
                Log($"Oculus path: {oculusPath}");

                // Get the paths for SteamVR executables
                var steamPaths = GetSteamPaths();
                if (steamPaths == null)
                {
                    Log("Steam paths not found.");
                    return;
                }

                string startupPath = steamPaths.Item1;
                string vrServerPath = steamPaths.Item2;
                Log($"Steam startup path: {startupPath}");
                Log($"Steam VR server path: {vrServerPath}");

                // Start SteamVR using vrstartup.exe
                Process.Start(startupPath);
                Log("Started SteamVR using vrstartup.exe");

                // Wait for a short duration to ensure vrserver.exe has started
                await Task.Delay(5000);

                // Monitor vrserver process
                var vrServerProcess = Process.GetProcessesByName("vrserver").FirstOrDefault(process => process.MainModule.FileName == vrServerPath);
                if (vrServerProcess != null)
                {
                    Log("Monitoring vrserver process.");
                    vrServerProcess.EnableRaisingEvents = true;

                    // When vrserver exits, kill the Oculus process
                    vrServerProcess.Exited += (sender, e) =>
                    {
                        Log("vrserver process exited.");
                        KillOculusServer(oculusPath);
                    };

                    // Wait for vrserver to exit
                    vrServerProcess.WaitForExit();
                }
                else
                {
                    Log("SteamVR vrserver not found. Exiting...");
                }
            }
            catch (Exception e)
            {
                Log($"An exception occurred: {e.Message}");
                MessageBox.Show($"An exception occured while attempting to find/start SteamVR...\n\nMessage: {e}");
            }
        }

        // Initialize the logging mechanism
        private static void InitializeLogging()
        {
            if (!Directory.Exists(Path.GetDirectoryName(logPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            }
        }

        // Log messages to the defined log file
        private static void Log(string message)
        {
            File.AppendAllText(logPath, DateTime.Now + ": " + message + "\n");
        }

        // Get the path for Oculus
        static string GetOculusPath()
        {
            string oculusPath = Environment.GetEnvironmentVariable("OculusBase");
            if (string.IsNullOrEmpty(oculusPath))
            {
                LogError("Oculus installation environment not found...");
                return null;
            }

            oculusPath = Path.Combine(oculusPath, @"Support\oculus-runtime\OVRServer_x64.exe");
            if (!File.Exists(oculusPath))
            {
                LogError("Oculus server executable not found...");
                return null;
            }

            return oculusPath;
        }

        // Get the paths for SteamVR executables
        public static Tuple<string, string> GetSteamPaths()
        {
            string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
            if (!File.Exists(openVrPath))
            {
                LogError("OpenVR Paths file not found... (Has SteamVR been run once?)");
                return null;
            }

            try
            {
                JavaScriptSerializer jss = new JavaScriptSerializer();
                string openvrJsonString = File.ReadAllText(openVrPath);
                dynamic openvrPaths = jss.DeserializeObject(openvrJsonString);

                string location = openvrPaths["runtime"][0].ToString();
                string startupPath = Path.Combine(location, @"bin\win64\vrstartup.exe");
                string serverPath = Path.Combine(location, @"bin\win64\vrserver.exe");

                if (!File.Exists(startupPath) || !File.Exists(serverPath))
                {
                    LogError("SteamVR executables not found... (Has SteamVR been run once?)");
                    return null;
                }

                return new Tuple<string, string>(startupPath, serverPath);
            }
            catch (Exception e)
            {
                LogError($"Corrupt OpenVR Paths file found... (Has SteamVR been run once?)\n\nMessage: {e}");
                return null;
            }
        }

        // Kill the Oculus process
        static void KillOculusServer(string oculusPath)
        {
            var ovrServerProcess = Process.GetProcessesByName("OVRServer_x64").FirstOrDefault(process => process.MainModule.FileName == oculusPath);
            if (ovrServerProcess != null)
            {
                ovrServerProcess.Kill();
                ovrServerProcess.WaitForExit();
            }
            else
            {
                LogError("Oculus runtime not found...");
            }
        }

        // Log errors and show a message box
        static void LogError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
