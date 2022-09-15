using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using System.Reflection;
using System.Windows.Forms;
using System.Configuration;
using Microsoft.Office.Tools.Ribbon;

namespace SyncAddin_Config
{
    public class ContactRecords
    {

        public int ContactId { get; set; }

        public string Email { get; set; }

        public string Title { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string OfficePhone { get; set; }

        public string MobilePhone { get; set; }

        public string Agency { get; set; }

        public string Sub_Agency { get; set; }

        public string Street1 { get; set; }

        public string Street2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Country { get; set; }

        public string UploadedDate { get; set; }



    }
    public class Miscellaneous
    {
        public int InfoId { get; set; }

        public string MiscellaneousText { get; set; }

        public string UploadedDate { set; get; }

    }


    class AzureSync
    {



        private string connectionstring = "";

        private static List<ContactRecords> ConvertDataTable<ContactRecords>(System.Data.DataTable dt)
        {
            List<ContactRecords> data = new List<ContactRecords>();
            foreach (DataRow row in dt.Rows)
            {
                ContactRecords item = GetItem<ContactRecords>(row);
                data.Add(item);
            }
            return data;
        }
        private static ContactRecords GetItem<ContactRecords>(DataRow dr)
        {
            Type temp = typeof(ContactRecords);
            ContactRecords obj = Activator.CreateInstance<ContactRecords>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (pro.Name == column.ColumnName)
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    else
                        continue;
                }
            }
            return obj;
        }


        public bool CheckConnectionString()
        {
            string strServer = ConfigurationManager.AppSettings.Get("keyServerName");
            string strDbName = ConfigurationManager.AppSettings.Get("keyDatabaseName");
            string strUserName = ConfigurationManager.AppSettings.Get("keyUserName");
            string strPassword = ConfigurationManager.AppSettings.Get("keyPassword");

            //string conStr = "data source=" + ConfigurationManager.AppSettings.Get("strServer") + ";initial catalog=" + ConfigurationManager.AppSettings.Get("strDbName") + ";user id=" + ConfigurationManager.AppSettings.Get("strUserName") + "; password=" + ConfigurationManager.AppSettings.Get("strPassword") + "; MultipleActiveResultSets=True;App=EntityFramework";
            string conStr = "data source=" + strServer + ";initial catalog=" + strDbName + ";user id=" + strUserName + "; password=" + strPassword + "; MultipleActiveResultSets=True;App=EntityFramework";
            try
            {
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();

                    connectionstring = conStr;
                    con.Close();
                    return true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
                
                connectionstring = "";
                return false;
               
               
            }

        }
        public void Sync()
        {
            try
            {
                if (CheckConnectionString())
                {
                    Microsoft.Office.Interop.Excel.Worksheet worksheet = Globals.ThisAddIn.Application.ActiveSheet;
                    Range usedRange = worksheet.UsedRange;
                    int nColumnsMax = 0;
                    String sText = "";
                    System.Data.DataTable dt = new System.Data.DataTable();
                    System.Data.DataTable dtMisc = new System.Data.DataTable();
                    int ContactId;
                    string UploadedDate = DateTime.Now.ToString();
                    if (usedRange.Rows.Count > 0)
                    {
                        //----< Read_Header >----
                        for (int iColumn = 1; iColumn <= usedRange.Columns.Count - 5; iColumn++)
                        {
                            Microsoft.Office.Interop.Excel.Range cell = usedRange.Cells[1, iColumn] as Range;
                            dt.Columns.Add(cell.Value2);
                            nColumnsMax = iColumn;
                        }
                        //----</ Read_Header >----
                        //dtMisc.Columns.Add("Misc");
                        //----< Read_DataRows >---

                        for (int iRow = 2; iRow <= usedRange.Rows.Count; iRow++)
                        {
                            for (int iColumn = 1; iColumn <= nColumnsMax; iColumn++)
                            {
                                dt.Rows.Add();
                                Microsoft.Office.Interop.Excel.Range cell = usedRange.Cells[iRow, iColumn] as Range;
                                String sValue = (cell.Value2 ?? "").ToString();
                                dt.Rows[iRow - 2][iColumn - 1] = sValue;
                            }
                        }
                        //----</ Read_DataRows >----

                        //for (int jColumn=0; jColumn <= usedRange.Count + 5; jColumn++)
                        //{
                        //    dtMisc.Rows.Add("jColumn");

                        //} 



                    }
                    string ConnectionString = connectionstring;

                    System.Data.DataTable dtPreviousRecord = new System.Data.DataTable();

                    //string query = "select count* from Contact where ";

                    //SqlConnection connPrev = new SqlConnection(ConnectionString);
                    //SqlCommand cmdPrev = new SqlCommand(query, connPrev);
                    //connPrev.Open();
                    //SqlDataAdapter da = new SqlDataAdapter(cmdPrev);
                    //da.Fill(dtPreviousRecord);
                    //connPrev.Close();
                    //da.Dispose();

                    Boolean IsSave = false;
                    List<ContactRecords> Contact = new List<ContactRecords>();
                    Contact = ConvertDataTable<ContactRecords>(dtPreviousRecord);

                    //int i = 0;               
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        List<ContactRecords> DuplicateRecords = new List<ContactRecords>();
                        List<Miscellaneous> Misc = new List<Miscellaneous>();
                        List<int> InfoIds = new List<int>();
                        using (SqlConnection con = new SqlConnection(ConnectionString))
                        {
                            SqlCommand cmd = new SqlCommand();
                            int i = 0;
                            //con.Open();

                            foreach (DataRow row in dt.Rows)
                            {
                                string cmdtext = "";
                                if (dt.Rows[i][0].ToString() != "")
                                {
                                    int count = 0;
                                    using (SqlConnection con1 = new SqlConnection(ConnectionString))
                                    {
                                        string query = "select count(ContactId) from Contact where Email= '" + dt.Rows[i]["Email"].ToString() + "'";
                                        SqlConnection connPrev = new SqlConnection(ConnectionString);
                                        SqlCommand cmdPrev = new SqlCommand(query, connPrev);
                                        connPrev.Open();
                                        count = Convert.ToInt32(cmdPrev.ExecuteScalar());
                                        connPrev.Close();
                                    }
                                    if (count > 0)
                                    {
                                        int modified = 0;
                                        using (SqlConnection con2 = new SqlConnection(ConnectionString))
                                        {
                                            string query = "select ContactId from Contact where Email= '" + dt.Rows[i]["Email"].ToString() + "'";
                                            SqlConnection connPrev = new SqlConnection(ConnectionString);
                                            SqlCommand cmdPrev = new SqlCommand(query, connPrev);
                                            connPrev.Open();
                                            modified = Convert.ToInt32(cmdPrev.ExecuteScalar());
                                            connPrev.Close();
                                            InfoIds.Add(modified);
                                        }

                                        int j = 0;
                                        for (int iter = nColumnsMax + 1; iter <= usedRange.Columns.Count; iter++)
                                        {
                                            Microsoft.Office.Interop.Excel.Range cell = usedRange.Cells[i + 2, iter] as Range;
                                            String sValue = (cell.Value2 ?? "").ToString();
                                            if (sValue != string.Empty)
                                            {
                                                Miscellaneous mis = new Miscellaneous();
                                                //string UploadedDate = DateTime.Now.ToString();
                                                mis.MiscellaneousText = sValue;
                                                mis.InfoId = modified;
                                                mis.UploadedDate = UploadedDate;
                                                Misc.Add(mis);
                                            }
                                            j++;
                                        }
                                        DuplicateRecords.Add(GetItem<ContactRecords>(row as DataRow));
                                        i++;
                                    }
                                    else
                                    {
                                        ContactRecords item = GetItem<ContactRecords>(row as DataRow);
                                        Contact.Add(item);
                                        //string UploadedDate = DateTime.Now.ToString();     
                                        cmdtext += "insert into Contact  (FirstName, LastName, Email, Title, Agency, Sub_Agency,  OfficePhone,  MobilePhone,  Street1,  Street2,  City,  State,  Zip,  Country,UploadedDate) Values";
                                        cmdtext += "('" + dt.Rows[i]["FirstName"].ToString().Replace("'","''") + "','" + dt.Rows[i]["LastName"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["Email"].ToString() + "','" + dt.Rows[i]["Title"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["Agency"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["Sub_Agency"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["Officephone"].ToString() + "','" + dt.Rows[i]["MobilePhone"].ToString() + "','" + dt.Rows[i]["Street1"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["Street2"].ToString().Replace("'", "''") + "','" + dt.Rows[i]["City"].ToString() + "','" + dt.Rows[i]["State"].ToString() + "','" + dt.Rows[i]["Zip"].ToString() + "','" + dt.Rows[i]["Country"].ToString() + "','" + DateTime.Now.ToString() + "');SELECT SCOPE_IDENTITY();";

                                        string val = "";
                                        int modified = 0;
                                        cmd.Connection = con;
                                        cmd.CommandText = cmdtext;
                                        //int modified = Convert.ToInt32(Cmd.ExecuteScalar());
                                        //int modified = Convert.ToInt32(cmd.ExecuteScalar());

                                        con.Open();
                                        modified = Convert.ToInt32(cmd.ExecuteScalar());

                                        con.Close();
                                        System.Data.DataTable MiscDt = new System.Data.DataTable();

                                        MiscDt.Columns.Add("InfoId");
                                        MiscDt.Columns.Add("MiscellaneousText");
                                        MiscDt.Columns.Add("UploadedDate", typeof(DateTime));



                                        //string UploadedDate = DateTime.Now.ToString();
                                        int j = 0;
                                        for (int iter = nColumnsMax + 1; iter <= usedRange.Columns.Count; iter++)
                                        {
                                            Microsoft.Office.Interop.Excel.Range cell = usedRange.Cells[i + 2, iter] as Range;
                                            String sValue = (cell.Value2 ?? "").ToString();
                                            if (sValue != string.Empty)
                                            {
                                                MiscDt.Rows.Add();
                                                MiscDt.Rows[j][1] = sValue;
                                                MiscDt.Rows[j][0] = modified;
                                                MiscDt.Rows[j][2] = UploadedDate;
                                            }
                                            j++;
                                        }

                                        int a = 0;
                                        foreach (var rowMisc in MiscDt.Rows)
                                        {

                                            //int a = 1;

                                            string MiscText = "";


                                            //var UploadedDate = DateTime.Now.ToString("dd/MM/yyyy");
                                            MiscText += "insert into Miscellaneous  (InfoId, MiscellaneousText,UploadedDate) Values";
                                            MiscText += "(" + Convert.ToInt32(MiscDt.Rows[a]["InfoId"]) + ",'" + MiscDt.Rows[a]["MiscellaneousText"].ToString().Replace("'", "''") + "','" + DateTime.Now.ToString() + "');";
                                            using (SqlConnection conn = new SqlConnection(ConnectionString))
                                            {
                                                SqlCommand cmd1 = new SqlCommand();
                                                cmd1.CommandText = MiscText;
                                                cmd1.Connection = conn;
                                                conn.Open();
                                                cmd1.ExecuteNonQuery();
                                                conn.Close();
                                            }
                                            a++;
                                        }
                                        MiscDt.Clear();
                                        MiscDt = null;
                                        i++;
                                        IsSave = true;
                                    }

                                    if (con.State == System.Data.ConnectionState.Open)
                                        con.Close();

                                }
                            }

                        }
                        bool updated = false;
                        if (DuplicateRecords != null && DuplicateRecords.Count > 0)
                        {
                            int CountOfDuplicates;
                            CountOfDuplicates = DuplicateRecords.Count();
                            DialogResult dialogResult = MessageBox.Show(CountOfDuplicates.ToString() + " records already exist. Click Yes to update records or No to skip.", "Confirmation", MessageBoxButtons.YesNo);

                            if (dialogResult == DialogResult.Yes)
                            {
                                using (SqlConnection con = new SqlConnection(connectionstring))

                                {
                                    con.Open();
                                    foreach (var d in DuplicateRecords)
                                    {

                                        SqlCommand cmd1 = new SqlCommand();
                                        //cmd1.CommandText = "Insert into Archive COntact (FirstName,LastName,Email,Title,Agency,SubAgency,officePhone,MobilePhone,Street1,Street2,City,State,Zip,Country )Values ("+d.FirstName+",'"+d.LastName+ "','" + d.Email + "','" + d.Title+ "','" + d.Agency+ "','" + d.Sub_Agency+ "','" + d.OfficePhone+ "','" + d.MobilePhone+ "','" + d.Street1+ "','" + d.Street2+ "','" + d.City+ "','" + d.State + "','" + d.Zip+ "','" + d.Country+ "')";
                                        cmd1.CommandText = "Insert into ArchiveContact " +
                                                            "select c.ContactId,c.FirstName,   c.LastName,c.Email,c.Title,c.Agency,c.Sub_Agency,c.OfficePhone,c.MobilePhone,c.Street1,c.Street2,c.City,c.State,c.Zip,c.Country, c.UploadedDate from Contact c where Email = '" + d.Email + "'; update Contact set FirstName='" + d.FirstName.ToString() + "',LastName='" + d.LastName.ToString() + "', Email='" + d.Email.ToString() + "',Title='" + d.Title.ToString() + "',Agency='" + d.Agency.ToString() + "',Sub_Agency='" + d.Sub_Agency.ToString() + "',OfficePhone='" + d.OfficePhone.ToString() + "',MobilePhone='" + d.MobilePhone.ToString() + "',Street1='" + d.Street1.ToString() + "',Street2='" + d.Street2.ToString() + "',City='" + d.City.ToString() + "',State='" + d.State.ToString() + "',Zip='" + d.Zip.ToString() + "',Country='" + d.Country + "',UploadedDate='" + DateTime.Now.ToString() + "' where Email='" + d.Email.ToString() + "'";
                                        cmd1.Connection = con;

                                        cmd1.ExecuteNonQuery();
                                        updated = true;

                                    }

                                    foreach (var d in InfoIds)
                                    {

                                        SqlCommand cmd1 = new SqlCommand();
                                        //cmd1.CommandText= "insert into Miscellaneous Archive(InfoId, MiscellaneousText) Values(" +d+ ", '" + d.MiscellaneousText + "')";
                                        cmd1.CommandText = "Insert into ArchiveMiscellaneousText " +
                                        "select c.InfoId,c.MiscellaneousText,c.UploadedDate from Miscellaneous c where InfoId=" + d + "; delete from Miscellaneous where InfoId=" + d;
                                        cmd1.Connection = con;
                                        cmd1.ExecuteNonQuery();
                                        updated = true;
                                    }
                                    string cmdMisc = "";
                                    foreach (var d in Misc)
                                    {

                                        cmdMisc += " insert into Miscellaneous  (InfoId, MiscellaneousText,UploadedDate) Values (" + Convert.ToInt32(d.InfoId) + ",'" + d.MiscellaneousText + "','" + DateTime.Now.ToString() + "' )  update contact set UploadedDate= '" + DateTime.Now.ToString() + "' where ContactId=" + d.InfoId + " ;";
                                    }
                                    SqlCommand cmd2 = new SqlCommand();

                                    cmd2.CommandText = cmdMisc;
                                    cmd2.Connection = con;
                                    cmd2.ExecuteNonQuery();
                                    updated = true;


                                    con.Close();
                                }
                            }

                        }
                        if (updated == true)
                        {
                            MessageBox.Show("Record has been updated successfully");
                        }
                        else
                        {
                            if (IsSave == true)
                            {
                                MessageBox.Show("Record has been Save successfully");
                            }
                            else
                            {
                                MessageBox.Show("No Record Updated");
                            }
                        }
                    }
                }
                else
                {
                    //MessageBox.Show("Invalid Credentials");
                    //Open Credentional task pane
                    //Globals.ThisAddIn.TaskPane.Visible = true;
                }
            }

            catch (Exception ex)
            {

            }
        }
    }
}
