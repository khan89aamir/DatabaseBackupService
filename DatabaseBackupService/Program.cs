using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Windows.Forms;
using System.Runtime.CompilerServices;
[assembly: SuppressIldasm]

namespace DatabaseBackupService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [Obsolete]
        static void Main()
        {
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName("DatabaseBackupService");
            if (p.Length > 1)
            {
                Application.Exit();
            }
            else
            {
                string myServiceName = System.Configuration.ConfigurationSettings.AppSettings["ServiceName"].ToString();
                string status; //service status (For example, Running or Stopped)
               
                //display service status: For example, Running, Stopped, or Paused
                ServiceController mySC = new ServiceController(myServiceName);
                try
                {
                    status = mySC.Status.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Service not found. It is probably not installed. [exception=" + ex.Message + "]");
                    return;
                }
                //display service status: For example, Running, Stopped, or Paused
                //MessageBox.Show("Service status : " + status);

                //if service is Stopped or StopPending, you can run it with the following code.
                if (mySC.Status.Equals(ServiceControllerStatus.Stopped) | mySC.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    try
                    {
                        mySC.Start();
                        mySC.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error in starting the service: " + ex.Message);
                    }
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmDatabackup());
            }           
        }
    }
}