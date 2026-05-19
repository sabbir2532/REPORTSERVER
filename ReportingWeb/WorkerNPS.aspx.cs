using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class WorkerNPS : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        SqlConnection con = new SqlConnection("Server=DESKTOP-AKDM1H7;Database=accounts;Integrated Security=True;TrustServerCertificate=True");
        SqlCommand cmd = new SqlCommand("select * from MasterTransWorkerNPS order by ccod", con);
        SqlDataAdapter sda = new SqlDataAdapter(cmd);
        DataSet ds = new DataSet();
        sda.Fill(ds);
        ReportDocument crp = new ReportDocument();
        crp.Load(Server.MapPath("~/WorkerNpsFile/PaySlipNPS_new.rpt"));
        crp.SetDataSource(ds.Tables["table"]);
        CrystalReportViewer1.ReportSource = crp;
        crp.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "Worker NPS");
    }
}