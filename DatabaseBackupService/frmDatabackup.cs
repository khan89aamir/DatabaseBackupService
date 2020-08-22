using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using CoreApp;
//using System.Diagnostics;

namespace DatabaseBackupService
{
    public partial class frmDatabackup : Form
    {
        clsConnection_DAL ObjDAL = new clsConnection_DAL(true);
        clsUtility ObjUtil = new clsUtility();

        RegistryKey rkApp;

        string strBackUpPath = string.Empty;
        string DatabaseName = string.Empty;
        int hr;
        int min;
        DateTime backupTime,backupDate;

        public frmDatabackup()
        {
            InitializeComponent();
            try
            {
                rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                rkApp.SetValue("BackupService", Application.ExecutablePath.ToString());
            }
            catch { }
        }

        private void BackUpDatabase()
        {
            try
            {
                string sqlcmd = string.Empty;

                sqlcmd = "BACKUP database " + DatabaseName + " TO DISK='" + strBackUpPath + @"\" + DatabaseName + ".bak' WITH INIT";
                ObjDAL.ExecuteNonQuery(sqlcmd);
                ObjDAL.UpdateColumnData("[Date]", SqlDbType.Date, DateTime.Now);
                int a = ObjDAL.UpdateData(DatabaseName + ".dbo.BackupConfig", "1=1");
                if (a > 0)
                {
                    WriteBackupLog(DatabaseName, "Backup Success");
                }
                else
                {
                    WriteBackupLog(DatabaseName, "Backup Faild");
                }

                System.Threading.Thread.Sleep(60000);
            }
            catch (Exception ex)
            {
                WriteBackupLog(DatabaseName, "Backup Faild\n Reason :" + ex.ToString());
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Size = new Size(1, 1);
                this.Hide();
                if (File.Exists(Application.StartupPath + "\\AppConfig/ServerConfig.sc"))
                {
                    //string arr = ObjDAL.ReadConStringFromFile(Application.StartupPath + "\\AppConfig/ServerConfig.sc", true);
                    //string[] a = arr.Split(new char[] { '=', ';' });
                    DatabaseName = ObjDAL.GetCurrentDBName(true);
                    WriteBackupLog(DatabaseName, "Auto Backup Service is Started..");

                    LoadData();
                    timer1.Enabled = true;
                    timer1.Start();
                }
                else
                {
                    WriteBackupLog(Environment.MachineName, "File is not exist for Auto Backup Service");
                    timer1.Stop();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                WriteBackupLog(DatabaseName, "Auto Backup service is closed \n Reason : " + ex.ToString());
                timer1.Stop();
                this.Close();
            }
        }

        private void LoadData()
        {
            try
            {
                DataTable dt = ObjDAL.ExecuteSelectStatement("SELECT TOP 1 Path, CONVERT(date,Date) as Date, LTRIM(RIGHT(CONVERT(VARCHAR(20), Time, 100), 7)) AS Time FROM " + DatabaseName + ".dbo.BackupConfig WITH(NOLOCK)");
                if (ObjUtil.ValidateTable(dt))
                {
                    strBackUpPath = dt.Rows[0]["Path"].ToString();
                    backupDate = Convert.ToDateTime(dt.Rows[0]["Date"]);
                    backupTime = Convert.ToDateTime(dt.Rows[0]["Time"]);
                    hr = backupTime.Hour;
                    min = backupTime.Minute;
                }
            }
            catch
            {
                timer1.Stop();
                this.Close();
            }
        }

        private void WriteBackupLog(string dbName, string status)
        {
            ObjUtil.WriteToFile("Log Date : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), Application.StartupPath + "\\backup.log", true);
            ObjUtil.WriteToFile("Database Name : " + dbName, Application.StartupPath + "\\backup.log", true);
            ObjUtil.WriteToFile("Status : " + status, Application.StartupPath + "\\backup.log", true);
            ObjUtil.WriteToFile("______________________________________", Application.StartupPath + "\\backup.log", true);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if ((DateTime.Now.Hour == hr && DateTime.Now.Minute == min) || backupDate.ToString("yyyy-MM-dd")!=DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    BackUpDatabase();
                    timer1.Stop();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                WriteBackupLog(DatabaseName, "Error\n Reason : " + ex.ToString());
                timer1.Stop();
                this.Close();
            }
        }
    }
}