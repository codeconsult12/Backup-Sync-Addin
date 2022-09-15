using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Data.SqlClient;

namespace SyncAddin_Config
{
    public partial class TaskPaneControl : UserControl
    {
        public TaskPaneControl()
        {
            InitializeComponent();
        }
        string ServerName;
        string DatabaseName;
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void TaskPaneControl_Load(object sender, EventArgs e)
        {
            //string ServerName;

             ServerName = ConfigurationManager.AppSettings["keyServerName"];
            DatabaseName = ConfigurationManager.AppSettings["keyDatabaseName"];

            txtServerName.Text = ServerName;
            txtDatabaseName.Text = DatabaseName;
        }

        private void BtnConnectCredintials_Click(object sender, EventArgs e)
        {
            try
            {
                //string ServerName;
                ServerName = txtServerName.Text;


                ////string DatabaseName;
                DatabaseName = txtDatabaseName.Text;

                string UserName;
                UserName = txtUserName.Text;


                string Password;
                Password = txtPassword.Text;

                


                ConfigurationManager.AppSettings.Set("keyServerName", ServerName);
                ConfigurationManager.AppSettings.Set("keyUserName", UserName);
                ConfigurationManager.AppSettings.Set("keyPassword", Password);
                ConfigurationManager.AppSettings.Set("keyDatabaseName", DatabaseName);
                using (SqlConnection con = new SqlConnection("data source=" + ConfigurationManager.AppSettings.Get("keyServerName") + ";initial catalog=" + ConfigurationManager.AppSettings.Get("keyDatabaseName") + ";user id=" + ConfigurationManager.AppSettings.Get("keyUserName") + "; password=" + ConfigurationManager.AppSettings.Get("keyPassword") + "; MultipleActiveResultSets=True;App=EntityFramework"))
                {
                    con.Open();
                    con.Close();
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid Database Credentials.");

            }
        }

    }
}
