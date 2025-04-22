using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TraceAbiltyMatrix
{
    public partial class TraceabilityMatrix : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string serialNumber = txtSerialNumber.Text.Trim();

            if (!string.IsNullOrEmpty(serialNumber))
            {
                DataTable dt = GetTraceabilityMatrix(serialNumber);
                gvTraceability.DataSource = dt;
                gvTraceability.DataBind();
            }
        }

        private DataTable GetTraceabilityMatrix(string serialNumber)
        {
            DataTable dt = new DataTable();

            string connStr = ConfigurationManager.AppSettings["strConnect"];

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
                SELECT * 
                FROM AssemblyMaster
                WHERE AssemblyID = (
                    SELECT TOP 1 AssemblyID 
                    FROM AssemblyMaster 
                    WHERE SerialNumber = @SerialNumber
                )
                AND BOM = (
                    SELECT TOP 1 BOM 
                    FROM AssemblyMaster 
                    WHERE SerialNumber = @SerialNumber
                )
                ORDER BY ItemType DESC, CreatedDate";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SerialNumber", serialNumber);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            return dt;
        }
    }
}