using Microsoft.Win32;
using Serilog;
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
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(
                    path: $"OculusKiller-{DateTime.Now:yyyyMMdd}.log",
                    fileSizeLimitBytes: 1000 * 500, // 500 KB aprox.
                    retainedFileCountLimit: 2)
                .CreateLogger();

            Log.Information("Initialize OculusKiller...");

            try
            {
                var steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null).ToString();
                if (string.IsNullOrEmpty(steamPath))
                {
                    throw new DirectoryNotFoundException("Steam installation not found. Searched at registry => HKEY_CURRENT_USER\"Software\"Valve\"Steam");
                }

                steamPath = Path.GetFullPath(steamPath);

                // Going to assume SteamVR is in the default steamapps path, this could be bad.
                var binaryPath = Path.Combine(steamPath, @"steamapps\common\SteamVR\bin\win64");
                var vrStartupPath = Path.Combine(binaryPath, "vrstartup.exe");
                if (Directory.Exists(binaryPath) && File.Exists(vrStartupPath))
                {
                    var vrStartupProcess = Process.Start(vrStartupPath);
                    vrStartupProcess.WaitForExit();
                    Log.Information("...OculusKiller run successfully.");
                    Log.CloseAndFlush();
                }
                else
                {
                    Log.Error($"Couldn't find SteamVR at {vrStartupPath}");
                    MessageBox.Show($"Couldn't find SteamVR! (Did you install it and run it once?)");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"An exception occured.");
                MessageBox.Show($"An exception occured while attempting to find SteamVR! (Did you install it and run it once?)\n\nMessage: {ex}");
            }
        }
    }
}
