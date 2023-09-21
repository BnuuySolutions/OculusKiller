using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Timers;
using System.Text;

namespace OculusSwapper
{
    static class Program
    {
        private static NotifyIcon? trayIcon;
        private static ContextMenuStrip? trayMenu;
        private static ToolStripMenuItem? versionMenuItem; // Menu item to display the version

        [STAThread]
        static void Main()
        {
            // Check if the application is running as an administrator
            if (!IsAdministrator())
            {
                // If not, restart the application with administrative privileges
                var processInfo = new ProcessStartInfo
                {
                    Verb = "runas",
                    FileName = Application.ExecutablePath
                };

                try
                {
                    Process.Start(processInfo);
                }
                catch
                {
                    MessageBox.Show("Failed to run as administrator.");
                }

                // Close the current instance
                Application.Exit();
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create a simple tray menu with options.
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Swap to Custom", null, SwapToCustom);
            trayMenu.Items.Add("Swap to OculusDash", null, SwapToOculusDash);
            trayMenu.Items.Add("Terminate OculusDash", null, TerminateOculusDash);
            trayMenu.Items.Add("Stop Oculus Runtimes", null, StopOculusRuntimes);
            trayMenu.Items.Add("Restart Oculus Runtimes", null, RestartOculusRuntimes);
            versionMenuItem = new ToolStripMenuItem { Enabled = false }; // Non-clickable menu item
            trayMenu.Items.Add(versionMenuItem);
            trayMenu.Items.Add("Exit", null, OnExit);
            trayMenu.Items.Add("About", null, OnAbout);

            // Load the embedded icon
            Icon? appIcon = null;
            using (var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OculusSwapper.Zuccverse.ico"))
            {
                if (iconStream != null)
                {
                    appIcon = new Icon(iconStream);
                }
                else
                {
                    Console.WriteLine("Failed to load embedded icon.");
                }
            }

            // Create the tray icon.
            trayIcon = new NotifyIcon()
            {
                Text = "OculusSwapper",
                Icon = appIcon,
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            // Check the current state once initially
            CheckCurrentState(null, null);

            Application.Run();
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void SwapToCustom(object? sender, EventArgs e)
        {
            string oculusBasePath = @"C:\Program Files\Oculus\Support";
            string originalOculusDashPath = Path.Combine(oculusBasePath, @"oculus-dash\dash\bin\OculusDash.exe");
            string backupOculusDashPath = Path.Combine(oculusBasePath, @"oculus-dash\dash\bin\OculusDash.exe.bk");
            string customOculusDashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OculusDash.exe");

            // Check if the custom OculusDash.exe exists in the same directory as OculusSwapper
            if (!File.Exists(customOculusDashPath))
            {
                MessageBox.Show("Error: OculusKiller OculusDash.exe not found in the same directory as OculusSwapper.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If OculusDash.exe.bk exists and is the original version
            if (File.Exists(backupOculusDashPath) && GetOculusVersion(backupOculusDashPath) != "Killer")
            {
                // If OculusDash.exe also exists, delete it
                if (File.Exists(originalOculusDashPath))
                {
                    File.Delete(originalOculusDashPath);
                }
            }
            else if (!File.Exists(backupOculusDashPath) && GetOculusVersion(originalOculusDashPath) != "Killer")
            {
                // If OculusDash.exe.bk doesn't exist and OculusDash.exe is the original, rename OculusDash.exe to OculusDash.exe.bk
                File.Move(originalOculusDashPath, backupOculusDashPath);
            }

            // Copy the custom OculusDash.exe to the original location
            File.Copy(customOculusDashPath, originalOculusDashPath, true);

            // Show a balloon notification
            trayIcon.BalloonTipTitle = "OculusSwapper";
            trayIcon.BalloonTipText = "Swapped to OculusKiller";
            trayIcon.ShowBalloonTip(2000); // Show for 2 seconds

            // Update the tray menu to reflect the change
            CheckCurrentState(null, null);
        }

        private static void SwapToOculusDash(object? sender, EventArgs e)
        {
            string oculusBasePath = @"C:\Program Files\Oculus\Support";
            string originalOculusDashPath = Path.Combine(oculusBasePath, @"oculus-dash\dash\bin\OculusDash.exe");
            string backupOculusDashPath = Path.Combine(oculusBasePath, @"oculus-dash\dash\bin\OculusDash.exe.bk");

            // Check if the backup OculusDash.exe.bk exists and is the original version
            if (!File.Exists(backupOculusDashPath) || GetOculusVersion(backupOculusDashPath) == "Killer")
            {
                MessageBox.Show("Error: Original OculusDash.exe backup not found or is not the original version.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // If the current OculusDash.exe is the custom version, attempt to delete it
            if (File.Exists(originalOculusDashPath) && GetOculusVersion(originalOculusDashPath) == "Killer")
            {
                int retries = 3;
                while (retries > 0)
                {
                    try
                    {
                        File.Delete(originalOculusDashPath);
                        break; // Exit the loop if the delete was successful
                    }
                    catch
                    {
                        if (retries == 1) // If this was the last retry, show an error message
                        {
                            MessageBox.Show("Error: Unable to delete OculusKiller OculusDash.exe. Please close any processes that might be using it and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        System.Threading.Thread.Sleep(1000); // Wait for 1 second before retrying
                        retries--;
                    }
                }
            }

            // Restore the original OculusDash.exe from the backup
            File.Move(backupOculusDashPath, originalOculusDashPath);

            // Show a balloon notification
            trayIcon.BalloonTipTitle = "OculusSwapper";
            trayIcon.BalloonTipText = "Swapped to Original OculusDash";
            trayIcon.ShowBalloonTip(2000); // Show for 2 seconds

            // Update the tray menu to reflect the change
            CheckCurrentState(null, null);
        }

        private static void OnExit(object? sender, EventArgs e)
        {
            trayIcon?.Dispose();
            Application.Exit();
        }

        static string? GetOculusDashPath()
        {
            string oculusBasePath = @"C:\Program Files\Oculus\Support";
            string oculusDashPath = Path.Combine(oculusBasePath, @"oculus-dash\dash\bin\OculusDash.exe");
            if (File.Exists(oculusDashPath))
            {
                return oculusDashPath;
            }
            return null;
        }

        static void CheckCurrentState(object? sender, ElapsedEventArgs? e)
        {
            var oculusDashPath = GetOculusDashPath();
            if (string.IsNullOrEmpty(oculusDashPath))
            {
                versionMenuItem.Text = "OculusDash.exe not found";
                return;
            }

            string? version = GetOculusVersion(oculusDashPath);
            if (version == "Killer")
            {
                versionMenuItem.Text = "OculusKiller is installed";
            }
            else
            {
                versionMenuItem.Text = "Original OculusDash is installed";
            }
        }

        static string? GetOculusVersion(string exePath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(exePath);
                string fileContent = Encoding.UTF8.GetString(fileBytes);

                const string searchKey = "OculusVersion";
                int startIndex = fileContent.IndexOf(searchKey);
                if (startIndex >= 0)
                {
                    int valueStartIndex = startIndex + searchKey.Length + 1; // +1 for the '=' character
                    int valueEndIndex = fileContent.IndexOf('\0', valueStartIndex); // Assuming the metadata value ends with a null terminator
                    if (valueEndIndex > valueStartIndex)
                    {
                        return fileContent.Substring(valueStartIndex, valueEndIndex - valueStartIndex);
                    }
                }
            }
            catch
            {
                // Handle exceptions if needed
            }
            return null;
        }

        private static void OnAbout(object? sender, EventArgs e)
        {
            string aboutText = "OculusSwapper\n" +
                               "Created by Eliminater74\n" +
                               "Year: 2023\n\n" +
                               "Instructions:\n" +
                               "1. Use 'Swap to Custom' to replace OculusDash.exe with the custom version.\n" +
                               "2. Use 'Swap to OculusDash' to restore the original OculusDash.exe.\n" +
                               "3. 'Exit' to close the application.";

            MessageBox.Show(aboutText, "About OculusSwapper", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static void TerminateOculusDash(object? sender, EventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("OculusDash"))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                    MessageBox.Show("Failed to terminate OculusDash.exe. Ensure you have the necessary permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void StopOculusRuntimes(object? sender, EventArgs e)
        {
            string[] oculusRuntimeProcesses = { "OVRRedir", "OVRServer_x64", "OVRServiceLauncher" };

            foreach (var processName in oculusRuntimeProcesses)
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch
                    {
                        MessageBox.Show($"Failed to terminate {processName}. Ensure you have the necessary permissions.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private static void RestartOculusRuntimes(object? sender, EventArgs e)
        {
            string oculusBasePath = @"C:\Program Files\Oculus\Support\oculus-runtime";
            string[] oculusRuntimes =
            {
        Path.Combine(oculusBasePath, "OVRRedir.exe"),
        Path.Combine(oculusBasePath, "OVRServer_x64.exe"),
        Path.Combine(oculusBasePath, "OVRServiceLauncher.exe")
            };

            foreach (var runtime in oculusRuntimes)
            {
                try
                {
                    Process.Start(runtime);
                }
                catch
                {
                    MessageBox.Show($"Failed to start {Path.GetFileName(runtime)}. Ensure you have the necessary permissions and the file exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
