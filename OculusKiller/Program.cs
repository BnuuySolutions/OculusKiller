using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace OculusKiller
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                // Get all (uninstallable) programs from the registry
                string programsKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                RegistryKey programsKey = Registry.LocalMachine.OpenSubKey(programsKeyPath);
                string[] programs = programsKey.GetSubKeyNames();

                // Loop through each program subkey
                foreach (string programId in programs)
                {
                    // Make sure we are launching SteamVR
                    if (programId != "Steam App 250820") // ProgramID should be according to https://steamdb.info/app/250820/
                    {
                        // Let the user know if we haven't found SteamVR
                        if (programId == programs[programs.Length - 1]) MessageBox.Show("SteamVR not found! (Is it installed?)");
                        continue;
                    }

                    // Get "InstallLocation", and combine it to get the binary location
                    RegistryKey programKey = programsKey.OpenSubKey(programId);
                    string programPath = programKey.GetValue("InstallLocation").ToString();
                    string binaryPath = Path.Combine(programPath, @"bin\win64"); // We have this for existence checking
                    string vrStartupPath = Path.Combine(binaryPath, "vrstartup.exe");

                    // Double check that the program is installed (not just in name)
                    if (Directory.Exists(binaryPath) && File.Exists(vrStartupPath))
                    {
                        // Start SteamVR
                        Process vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                    {
                        // The program appears as installed, but points to the wrong directory...
                        MessageBox.Show("SteamVR location existence mismatch! (Is SteamVR done installing?)");
                    }

                    // Since we've found SteamVR, stop looping
                    break;
                }
            }
            catch (Exception ex)
            {
                // Something went wrong, give a general message with the error
                MessageBox.Show($"An exception occured while attempting to find and launch SteamVR! (Did you install it and run it once?)\n\nMessage: {ex}");
            }
        }
    }
}
