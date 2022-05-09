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
                // Get all (uninstallable) programs
                string programsKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                RegistryKey programsKey = Registry.LocalMachine.OpenSubKey(programsKeyPath);
                string[] programs = programsKey.GetSubKeyNames();

                // Loop through each program entry
                foreach (string programId in programs)
                {
                    // Get the program's name
                    RegistryKey subkey = programsKey.OpenSubKey(programId);
                    object programNameObj = subkey.GetValue("DisplayName");
                    if (programNameObj == null) continue; // Some programs don't exist?

                    // Check DisplayName (for insurance) and ProgramID (subkey name)
                    string programName = programNameObj.ToString();
                    if (programName != "SteamVR" || programId != "Steam App 250820") // ProgramID should be according to https://steamdb.info/app/250820/
                    {
                        // Let the user know if we haven't found SteamVR
                        if (programId == programs[programs.Length - 1]) MessageBox.Show("SteamVR not found! (Is it installed?)");
                        continue;
                    };

                    // Get the InstallLocation, and combine it to get the binary
                    string programPath = subkey.GetValue("InstallLocation").ToString();
                    string binaryPath = Path.Combine(programPath, @"bin\win64"); // We have this for existence checking
                    string vrStartupPath = Path.Combine(binaryPath, "vrstartup.exe");

                    // Double check that the program is installed (not just in name)
                    if (Directory.Exists(binaryPath) && File.Exists(vrStartupPath))
                    {
                        // Start SteamVR
                        var vrStartupProcess = Process.Start(vrStartupPath);
                        vrStartupProcess.WaitForExit();
                    }
                    else
                    {
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
