using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace OculusKillerInstaller
{
    public partial class Form1 : Form
    {

        string directory = "";
        int canStart = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            Process[] pname = Process.GetProcessesByName("OVRServer_x64");
            if (pname.Length == 0)
            {
                label2.Text = "Please start Oculus link APP !";
                closeAPPAsync();
                return;
            }
            else
            {
                directory = pname[0].MainModule.FileName;
                directory = Path.GetFullPath(Path.Combine(directory, @"..\..\"));
                directory += "oculus-dash\\dash\\bin";
            }

            if (directory == "") // check if directory exist
            {
                label2.Text = "OCULUS DIRECTORY IS NOW AVAILABLE, PLEASE START OCULUS LINK APP !";
                closeAPPAsync();
                return;
            }

            canStart = 1;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            MoveForm.MouseDown(sender);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            MoveForm.MouseMove(sender);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            MoveForm.MouseUp();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (canStart == 1)
            {
                startAsync();
            }
        }

        public async Task closeAPPAsync()
        {
            int val = 6;
            while (val >= 0)
            {
                val -= 1;
                await Task.Delay(1000);
                label2.Text = "Closing App in : " + val + "s";
            }
            Environment.Exit(0);
        }

        public async Task startAsync()
        {

            string nameAPP = "/OculusDash.exe";
            string nameAPPBACKUP = "/OculusDash.exe";

            try
            {

                if (AppDomain.CurrentDomain.BaseDirectory.Contains(@"Oculus\Support\oculus-dash\dash\bin") == false) //if APP is running inside the directory don't need to copy file
                {

                    //CHECK IF SERVICE IS RUNNING
                    ServiceController service = new ServiceController("OVRService");
                    if ((service.Status.Equals(ServiceControllerStatus.Stopped)) ||
                        (service.Status.Equals(ServiceControllerStatus.StopPending))) { }
                    else service.Stop();

                    while (true)
                    {
                        service = new ServiceController("OVRService");
                        if (service.Status == ServiceControllerStatus.Stopped)
                        {
                            if (File.Exists(directory + nameAPPBACKUP) == false)
                            {
                                File.Move(directory + nameAPP, directory + nameAPPBACKUP); //BACKUP FILE
                            }
                            if (File.Exists(directory + nameAPP))
                            {
                                File.Delete(directory + nameAPP);
                            }

                            File.Copy(AppDomain.CurrentDomain.FriendlyName, directory + nameAPP);

                            break;
                        }
                    }
                }

                label2.Text = "Getting OpenVr Path";
                await Task.Delay(500);

                string openVrPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"openvr\openvrpaths.vrpath");
                if (File.Exists(openVrPath))
                {

                    label2.Text = "Getting SteamVR path";
                    await Task.Delay(500);

                    var jss = new JavaScriptSerializer();
                    string jsonString = File.ReadAllText(openVrPath);
                    dynamic steamVrPath = jss.DeserializeObject(jsonString);

                    string vrStartupPath = Path.Combine(steamVrPath["runtime"][0].ToString(), @"bin\win64\vrstartup.exe");
                    if (File.Exists(vrStartupPath))
                    {
                        label2.Text = "Checking if app is already running";
                        await Task.Delay(500);

                        Process[] pname = Process.GetProcessesByName("vrserver");
                        if (pname.Length == 0)
                        {
                            label2.Text = "Starting SteamVR";
                            await Task.Delay(500);

                            Process vrStartupProcess = Process.Start(vrStartupPath);
                            vrStartupProcess.WaitForExit();
                        }
                        else
                        {
                            label2.Text = "SteamVR is already running !";
                            await Task.Delay(500);
                            closeAPPAsync();
                            return;
                        }
                    }
                    else
                        MessageBox.Show("SteamVR does not exist in installation directory.");
                }
                else
                    MessageBox.Show("Could not find openvr config file within LocalAppdata.");

                closeAPPAsync();
                return;
            }
            catch (Exception e)
            {
                MessageBox.Show($@"An exception occured while attempting to find SteamVR!\n\nMessage: {e}");
            }
        }
    }
}
