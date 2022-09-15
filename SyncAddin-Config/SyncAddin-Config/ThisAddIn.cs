using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Excel = Microsoft.Office.Interop.Excel;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Tools.Excel;

namespace SyncAddin_Config
{
    public partial class ThisAddIn
    {
        private TaskPaneControl taskPaneControl1;
        private Microsoft.Office.Tools.CustomTaskPane taskPaneValue;

        //private MyUserControl myUserControl1;
        //private Microsoft.Office.Tools.CustomTaskPane myCustomTaskPane;
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            taskPaneControl1 = new TaskPaneControl();
            taskPaneValue = this.CustomTaskPanes.Add(
                taskPaneControl1, "Change Credentials");
            taskPaneValue.VisibleChanged +=
                new EventHandler(taskPaneValue_VisibleChanged);
            //add-in With Position
            taskPaneValue.DockPosition =
                Office.MsoCTPDockPosition.msoCTPDockPositionRight;
            taskPaneValue.Width = 300;

            //taskPaneValue.DockPosition =
            //  Office.MsoCTPDockPosition.msoCTPDockPositionLeft;
            //taskPaneValue.Control.Left = 300;

            ////taskPaneValue.Control.= 50;
            //taskPaneValue.Width = 300;
            ////taskPaneValue.Height= 300;
        
            //add-in With Position










        }

        private void taskPaneValue_VisibleChanged(object sender, System.EventArgs e)
        {
            Globals.Ribbons.ManageTaskPaneRibbon.BtnSetupCredintials.Checked =
                taskPaneValue.Visible;
        }


        public Microsoft.Office.Tools.CustomTaskPane TaskPane
        {
            get
            {
                return taskPaneValue;
            }
        }
        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
        }

        #region VSTO generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }
        
        #endregion
    }
}
