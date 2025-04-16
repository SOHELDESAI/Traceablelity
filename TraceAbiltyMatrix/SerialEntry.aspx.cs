

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
                SELECT BOMDesc, ItemDesc, StockStatus FROM RecursiveBOM ORDER BY BOMLevel;
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
    }
}

