using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.UI;


namespace TraceAbiltyMatrix
{
    public partial class SerialEntry : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void gvBOM_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string stockStatus = DataBinder.Eval(e.Row.DataItem, "StockStatus").ToString();
                TextBox txtQty = (TextBox)e.Row.FindControl("txtQuantity");

                if (stockStatus == "M" || stockStatus == "F")
                {
                    txtQty.Visible = true;
                }
                else
                {
                    txtQty.Visible = false;
                }
            }
        }

        [WebMethod]
        public static List<string> GetBOMSuggestions(string prefix)
        {
            List<string> suggestions = new List<string>();
            string connStr = ConfigurationManager.AppSettings["strConnect"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT DISTINCT BOM FROM ERP_BOMHdr WHERE BOM LIKE @prefix + '%'", conn))
                {
                    cmd.Parameters.AddWithValue("@prefix", prefix);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        suggestions.Add(reader["BOM"].ToString());
                    }
                }
            }
            return suggestions;
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string selectedBOM = txtBOM.Text.Trim();
            if (!string.IsNullOrEmpty(selectedBOM))
            {
                LoadBOMData(selectedBOM);
            }
        }
        private void LoadBOMData(string bom)
        {
            string connStr = ConfigurationManager.AppSettings["strConnect"];
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                WITH LatestBOM AS (
                    SELECT BOM, MAX(BOMRevNo) AS LatestRevNo
                    FROM ERP_BOMHdr
                    WHERE GETDATE() BETWEEN BOMRevEffectiveDate AND BOMRevValidUptoDate
                    GROUP BY BOM
                ),
                RecursiveBOM AS (
                    SELECT  
                        HDR.BOM,  
                        BOMItemMaster.Description AS BOMDesc,  
                        DET.CompItem,  
                        CompItemMaster.Description AS ItemDesc,  
                        CompItemMaster.StockStatus AS StockStatus,  
                        0 AS BOMLevel  
                    FROM ERP_BOMHdr AS HDR
                    INNER JOIN LatestBOM AS LB ON HDR.BOM = LB.BOM AND HDR.BOMRevNo = LB.LatestRevNo
                    INNER JOIN ERP_ItemMaster AS BOMItemMaster ON HDR.BOM = BOMItemMaster.Item  
                    INNER JOIN ERP_BOMCompDetails AS DET ON HDR.BOM = DET.BOM AND HDR.BOMRevNo = DET.BOMRevNo  
                    INNER JOIN ERP_ItemMaster AS CompItemMaster ON DET.CompItem = CompItemMaster.Item  
                    WHERE HDR.BOM = @bom  

                    UNION ALL      

                    SELECT  
                        RB.BOM,  
                        RB.BOMDesc,  
                        DET.CompItem,  
                        CompItemMaster.Description AS ItemDesc,  
                        CompItemMaster.StockStatus AS StockStatus,  
                        RB.BOMLevel + 1  
                    FROM RecursiveBOM AS RB  
                    INNER JOIN ERP_BOMCompDetails AS DET ON RB.CompItem = DET.BOM  
                    INNER JOIN ERP_ItemMaster AS CompItemMaster ON DET.CompItem = CompItemMaster.Item  
                )
                SELECT BOMDesc, ItemDesc, StockStatus FROM RecursiveBOM WHERE StockStatus IN ('M', 'F') ORDER BY BOMLevel;
            ", conn))
                {
                    cmd.Parameters.AddWithValue("@bom", bom);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvBOM.DataSource = dt;
                    gvBOM.DataBind();
                }
            }
        }
        private string GenerateAssemblyID()
        {
            string connStr = ConfigurationManager.AppSettings["strConnect"];
            string newID = "ASM0001";

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT MAX(AssemblyID) FROM AssemblyMaster", conn);
                var result = cmd.ExecuteScalar();

                if (result != DBNull.Value && result != null)
                {
                    string lastID = result.ToString();
                    int number = int.Parse(lastID.Substring(3));
                    newID = "ASM" + (number + 1).ToString("D4");
                }
            }
            return newID;
        }
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string connStr = ConfigurationManager.AppSettings["strConnect"];
            string assemblyID = GenerateAssemblyID();

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    string fgSerial = txtFGSerial.Text.Trim();  

                    using (SqlCommand cmdFG = new SqlCommand(@"INSERT INTO AssemblyMaster 
                            (AssemblyID, BOM, ItemDesc, ItemType, StockStatus, SerialNumber) 
                            VALUES (@AssemblyID, @BOM, @ItemDesc, @ItemType, @StockStatus, @SerialNumber)", conn, trans))
                    {
                        cmdFG.Parameters.AddWithValue("@AssemblyID", assemblyID);
                        cmdFG.Parameters.AddWithValue("@BOM", txtBOM.Text.Trim());
                        cmdFG.Parameters.AddWithValue("@ItemDesc", txtBOM.Text.Trim()); 
                        cmdFG.Parameters.AddWithValue("@ItemType", "FinishGood");
                        cmdFG.Parameters.AddWithValue("@StockStatus", "F");
                        cmdFG.Parameters.AddWithValue("@SerialNumber", fgSerial);
                        cmdFG.ExecuteNonQuery();
                    }

                    foreach (GridViewRow row in gvBOM.Rows)
                    {
                        TextBox txtQty = (TextBox)row.FindControl("txtQuantity");
                        if (txtQty != null && txtQty.Visible && !string.IsNullOrEmpty(txtQty.Text))
                        {
                            string itemDesc = row.Cells[1].Text;
                            string stockStatus = row.Cells[2].Text;
                            string serialNumber = txtQty.Text.Trim();

                            string itemType = (stockStatus == "M") ? "SemiFinished" : "Raw";

                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO AssemblyMaster 
                        (AssemblyID, BOM, ItemDesc, ItemType, StockStatus, SerialNumber) 
                        VALUES (@AssemblyID, @BOM, @ItemDesc, @ItemType, @StockStatus, @SerialNumber)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@AssemblyID", assemblyID);
                                cmd.Parameters.AddWithValue("@BOM", txtBOM.Text.Trim());
                                cmd.Parameters.AddWithValue("@ItemDesc", itemDesc);
                                cmd.Parameters.AddWithValue("@ItemType", itemType);
                                cmd.Parameters.AddWithValue("@StockStatus", stockStatus);
                                cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                    trans.Commit();
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Assembly saved with ID: " + assemblyID + "');", true);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error: " + ex.Message + "');", true);
                }
            }
        }

    }
}

