using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace OculusKiller
{
    class Oculus
    {
        public string location;
        public string serverPath;

        public bool Update()
        {
            // We can get Oculus's installation path through the OculusBase environment variable!
            location = Environment.GetEnvironmentVariable("OculusBase");
            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                MessageBox.Show("Oculus installation environment not found...");
                return false;
            }

            // Try to get the path of the Oculus server... This also serves as insurance that Oculus.location is valid!
            serverPath = Path.Combine(location, @"Support\oculus-runtime\OVRServer_x64.exe");
            if (string.IsNullOrEmpty(serverPath))
            {
                MessageBox.Show("Oculus server executable not found...");
                return false;
            }

            return true;
        }

        public bool IsServer(Process serverProcess)
        {
            return serverProcess.MainModule.FileName == serverPath;
        }
    }
}
