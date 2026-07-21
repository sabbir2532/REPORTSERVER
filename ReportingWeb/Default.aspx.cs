using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class _Default : Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            if (Request.QueryString["action"] == "pdf")
            {
                GenerateReport();
            }
            else if (Request.QueryString["action"] == "bankst")
            {
                GenerateBankSt();
            }
            else if (Request.QueryString["action"] == "bankletter")
            {
                GenerateBankLetter();
            }
            else if (Request.QueryString["action"] == "bill")
            {
                GenerateBill();
            }
            else if (Request.QueryString["action"] == "tax")
            {
                GenerateTax();
            }
        }

    }


    private void GenerateReport()
    {
        ReportDocument crp = null;

        try
        {
            // =========================================
            // 1. RECEIVE PARAMETERS
            // =========================================

            string empno = Request.QueryString["empno"];
            string mnt = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];
            string empCategory = Request.QueryString["empCategory"];

            // =========================================
            // 2. VALIDATION
            // =========================================

            if (string.IsNullOrWhiteSpace(dbName))
                throw new Exception("Database name missing.");

            if (string.IsNullOrWhiteSpace(mnt))
                throw new Exception("Month missing.");

            if (string.IsNullOrWhiteSpace(year))
                throw new Exception("Year missing.");

            if (string.IsNullOrWhiteSpace(reportName))
                reportName = "PaySlipOfficer_LHB";
            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("empCategory missing");

            // =========================================
            // 3. CONNECTION STRING
            // =========================================

            string connectionString =
     $"Server=103.7.112.190,1433;" +
     $"Database={dbName};" +
     $"User Id=myuser;" +
     $"Password=1234;" +
     $"TrustServerCertificate=True;";

            // =========================================
            // 4. DYNAMIC TABLE
            // =========================================

            string tableName = "";
            string reportPath = "";
                
            if (empCategory == "OFFICER")
            {

                tableName = $"MasterOfficer_Pay_{mnt}_{year}";
                reportName = "PaySlipOfficer_LHB";
                reportPath = Server.MapPath(
                    $"~/{dbName}/{reportName}.rpt");
            }
            else if (empCategory == "STAFF")
            {
                // TABLE
                tableName = $"MasterStaff_Pay_{mnt}_{year}";
                reportName = "PaySlipOfficer_Staff";
                reportPath = Server.MapPath(
                    $"~/{dbName}/{reportName}.rpt");
            }
            else if (empCategory == "NPS")
            {
                // TABLE
                tableName = $"MasterWorkerNps_Pay_{mnt}_{year}";
                reportName = "PaySlipOfficer_NPS";
                reportPath = Server.MapPath(
                    $"~/{dbName}/{reportName}.rpt");
            }
            else if (empCategory == "WAGES")
            {
                // TABLE
                tableName = $"MasterWorkerWages_Pay_{mnt}_{year}";
                reportName = "PaySlipOfficer_Wages";
                reportPath = Server.MapPath(
                    $"~/{dbName}/{reportName}.rpt");
            }

            // =========================================
            // 5. QUERY
            // =========================================

            string query = $@"
                SELECT *
                FROM [{tableName}]
                WHERE
                    (@empno IS NULL OR empno = @empno)
                ORDER BY ccod, empno";

            DataSet ds = new DataSet();

            using (SqlConnection con =
                new SqlConnection(connectionString))
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@empno",
                        string.IsNullOrWhiteSpace(empno)
                            ? (object)DBNull.Value
                            : empno);

                    SqlDataAdapter sda =
                        new SqlDataAdapter(cmd);

                    sda.Fill(ds);
                }
            }

            // =========================================
            // 6. CHECK DATA EXISTS
            // =========================================

            if (ds.Tables.Count == 0 ||
                ds.Tables[0].Rows.Count == 0)
            {
                throw new Exception("No data found.");
            }

            // =========================================
            // 7. REPORT PATH
            // =========================================

            //string reportPath =
            //    Server.MapPath(
            //        $"~/{dbName}/{reportName}.rpt");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new Exception(
                    "Crystal Report file not found.");
            }

            // =========================================
            // 8. LOAD REPORT
            // =========================================

            crp = new ReportDocument();

            crp.Load(reportPath);

            crp.SetDataSource(ds.Tables[0]);

            // =========================================
            // 9. EXPORT PDF
            // =========================================

            Response.Clear();
            Response.Buffer = false;

            crp.ExportToHttpResponse(
                ExportFormatType.PortableDocFormat,
                Response,
                false,
                "OfficerReport");
        }
        catch (Exception ex)
        {
            Response.Clear();

            Response.ContentType = "text/html";

            Response.Write($@"
                <html>
                <head>
                    <title>Report Error</title>

                    <style>
                        body {{
                            font-family: Arial;
                            background: #f5f5f5;
                            padding: 40px;
                        }}

                        .error-box {{
                            background: white;
                            padding: 30px;
                            border-radius: 10px;
                            box-shadow: 0 0 10px #ccc;
                            max-width: 700px;
                            margin: auto;
                        }}

                        .title {{
                            color: red;
                            font-size: 24px;
                            margin-bottom: 15px;
                        }}

                        .msg {{
                            color: #333;
                            font-size: 18px;
                        }}
                    </style>
                </head>

                <body>

                    <div class='error-box'>

                        <div class='title'>
                            Report Loading Failed
                        </div>

                        <div class='msg'>
                            {ex.Message}
                        </div>

                    </div>

                </body>
                </html>");
        }
        finally
        {
            // =========================================
            // 10. CLEANUP
            // =========================================

            if (crp != null)
            {
                crp.Close();
                crp.Dispose();
            }
        }
    }

    private void GenerateBankSt()
    {
        ReportDocument crp = null;

        try
        {
            // PARAMETERS
            string bankName = Request.QueryString["bankName"];
            string mnt = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];
            string empCategory = Request.QueryString["empCategory"];

            if (string.IsNullOrEmpty(reportName))
                reportName = "BankStatement";

            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database missing");
            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("empCategory missing");

            // CONNECTION
            string connectionString =
       $"Server=103.7.112.190,1433;" +
       $"Database={dbName};" +
       $"User Id=myuser;" +
       $"Password=1234;" +
       $"TrustServerCertificate=True;";
            string tableName = "";
            if (empCategory== "OFFICER")
            {
                 tableName = $"MasterOfficer_Pay_{mnt}_{year}";
            }
            else if(empCategory== "STAFF")
            {
                // TABLE
                tableName = $"MasterStaff_Pay_{mnt}_{year}";
            }
            else if (empCategory == "NPS")
            {
                // TABLE
                tableName = $"MasterWorkerNps_Pay_{mnt}_{year}";
            }
            else if (empCategory == "WAGES")
            {
                // TABLE
                tableName = $"MasterWorkerWages_Pay_{mnt}_{year}";
            }

            // QUERY
            string query = $@"
            SELECT *
            FROM [{tableName}]
            WHERE BNKNAM = @bankName
            ";

            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@bankName", bankName);

                    SqlDataAdapter sda = new SqlDataAdapter(cmd);
                    sda.Fill(ds);
                }
            }

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No data found");

            // REPORT
            crp = new ReportDocument();

            string path = Server.MapPath($"~/{dbName}/{reportName}.rpt");

            crp.Load(path);
            crp.SetDataSource(ds.Tables[0]);

            // EXPORT
            Response.Clear();
            Response.Buffer = false;

            crp.ExportToHttpResponse(
                ExportFormatType.PortableDocFormat,
                Response,
                false,
                "BankStatement"
            );
        }
        catch (Exception ex)
        {
            Response.Clear();
            Response.Write($"<h3>Error</h3>{ex.Message}");
        }
        finally
        {
            if (crp != null)
            {
                crp.Close();
                crp.Dispose();
            }
        }
    }

    private void GenerateBankLetter()
    {
        ReportDocument crp = null;

        try
        {
            // =========================================
            // PARAMETERS
            // =========================================

            string bankName =
                Request.QueryString["bankName"];

            string empCategory =
                Request.QueryString["empCategory"];

            string mnt =
                Request.QueryString["mnt"];

            string year =
                Request.QueryString["year"];

            string dbName =
                Request.QueryString["key"];

            string reportName =
                Request.QueryString["reportName"];

            // =========================================
            // VALIDATION
            // =========================================

            if (string.IsNullOrEmpty(reportName))
                reportName = "BankLetterAgrani"; 

            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database missing");

            if (string.IsNullOrEmpty(bankName))
                throw new Exception("Bank Name missing");

            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("Emp Category missing");

            if (string.IsNullOrEmpty(mnt))
                throw new Exception("Month missing");

            if (string.IsNullOrEmpty(year))
                throw new Exception("Year missing");

            // =========================================
            // CONNECTION
            // =========================================

            string connectionString =
      $"Server=103.7.112.190,1433;" +
      $"Database={dbName};" +
      $"User Id=myuser;" +
      $"Password=1234;" +
      $"TrustServerCertificate=True;";

            // =========================================
            // TABLE NAME
            // =========================================

            string tableName = "";

            if (empCategory == "OFFICER")
            {
                tableName =
                    $"MasterOfficer_Pay_{mnt}_{year}";
            }
            else if (empCategory == "STAFF")
            {
                tableName =
                    $"MasterStaff_Pay_{mnt}_{year}";
            }
            else if (empCategory == "NPS")
            {
                tableName =
                    $"MasterWorkerNps_Pay_{mnt}_{year}";
            }
            else if (empCategory == "WAGES")
            {
                tableName =
                    $"MasterWorkerWages_Pay_{mnt}_{year}";
            }
            else
            {
                throw new Exception(
                    "Invalid Emp Category");
            }

            // =========================================
            // QUERY
            // =========================================

            string query = $@"
            SELECT *
            FROM [{tableName}]
            WHERE BNKNAM = @bankName
            ORDER BY ccod, empno";

            DataSet ds = new DataSet();

            using (SqlConnection con =
                new SqlConnection(connectionString))
            {
                using (SqlCommand cmd =
                    new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue(
                        "@bankName",
                        bankName);

                    SqlDataAdapter sda =
                        new SqlDataAdapter(cmd);

                    sda.Fill(ds);
                }
            }

            // =========================================
            // CHECK DATA
            // =========================================

            if (ds.Tables.Count == 0 ||
                ds.Tables[0].Rows.Count == 0)
            {
                throw new Exception(
                    "No data found.");
            }

            // =========================================
            // REPORT
            // =========================================

            crp = new ReportDocument();

            string reportPath =
                Server.MapPath(
                    $"~/{dbName}/{reportName}.rpt");

            if (!System.IO.File.Exists(reportPath))
            {
                throw new Exception(
                    "Report file not found.");
            }

            crp.Load(reportPath);

            crp.SetDataSource(ds.Tables[0]);

            // =========================================
            // EXPORT PDF
            // =========================================

            Response.Clear();
            Response.Buffer = false;

            crp.ExportToHttpResponse(
                ExportFormatType.PortableDocFormat,
                Response,
                false,
                "BankLetter");
        }
        catch (Exception ex)
        {
            Response.Clear();

            Response.ContentType = "text/html";

            Response.Write($@"
            <html>
            <body style='font-family:Arial;padding:20px;'>

                <h2 style='color:red;'>
                    Bank Letter Report Error
                </h2>

                <hr/>

                <p>
                    {ex.Message}
                </p>

            </body>
            </html>");
        }
        finally
        {
            if (crp != null)
            {
                crp.Close();
                crp.Dispose();
            }
        }
    }
    private void GenerateBill()
    {
        ReportDocument crp = null;

        try
        {
            // =====================================
            // PARAMETERS
            // =====================================

            string empno = Request.QueryString["empno"];
            string empCategory = Request.QueryString["empCategory"];
            string mnt = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];

            if (string.IsNullOrEmpty(reportName))
                reportName = "BillSum";

            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database missing");

            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("Emp Category missing");

            // =====================================
            // CONNECTION
            // =====================================

            string connectionString =
       $"Server=103.7.112.190,1433;" +
       $"Database={dbName};" +
       $"User Id=myuser;" +
       $"Password=1234;" +
       $"TrustServerCertificate=True;";

            // =====================================
            // TABLE NAME
            // =====================================

            string tableName = "";

            if (empCategory == "OFFICER")
                tableName = $"MasterOfficer_Pay_{mnt}_{year}";
            else if (empCategory == "STAFF")
                tableName = $"MasterStaff_Pay_{mnt}_{year}";
            else if (empCategory == "NPS")
                tableName = $"MasterWorkerNps_Pay_{mnt}_{year}";
            else if (empCategory == "WAGES")
                tableName = $"MasterWorkerWages_Pay_{mnt}_{year}";
            else
                throw new Exception("Invalid Category");

            // =====================================
            // QUERY
            // =====================================

            string query = $@"
            SELECT *
            FROM [{tableName}]
            WHERE (@empno IS NULL OR empno = @empno)
            ORDER BY ccod, empno
        ";

            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@empno",
                    string.IsNullOrEmpty(empno) ? (object)DBNull.Value : empno);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new Exception("No data found");

            // =====================================
            // REPORT LOAD
            // =====================================

            string reportPath =
                Server.MapPath($"~/{dbName}/{reportName}.rpt");

            if (!System.IO.File.Exists(reportPath))
                throw new Exception("Report file not found");

            crp = new ReportDocument();
            crp.Load(reportPath);
            crp.SetDataSource(ds.Tables[0]);

            // =====================================
            // EXPORT PDF
            // =====================================

            Response.Clear();
            Response.Buffer = false;

            crp.ExportToHttpResponse(
                ExportFormatType.PortableDocFormat,
                Response,
                false,
                "BillReport"

            );
        }
        catch (Exception ex)
        {
            Response.Clear();
            Response.ContentType = "text/html";
            Response.Write($"<h3>Bill Report Error</h3>{ex.Message}");
        }
        finally
        {
            if (crp != null)
            {
                crp.Close();
                crp.Dispose();
            }
        }
    }
    private void GenerateTax()
    {
        ReportDocument crp = null;

        try
        {
            // =====================================
            // PARAMETERS
            // =====================================

            string empno = Request.QueryString["empno"];
            string empCategory = Request.QueryString["empCategory"];
            string type = Request.QueryString["Type"];
            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];

            if (string.IsNullOrEmpty(reportName))
                reportName = "TaxCalculation_officeR";

            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database missing");

            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("Emp Category missing");

            // =====================================
            // CONNECTION
            // =====================================

            string connectionString =
       $"Server=103.7.112.190,1433;" +
       $"Database={dbName};" +
       $"User Id=myuser;" +
       $"Password=1234;" +
       $"TrustServerCertificate=True;";

            // =====================================
            // TABLE NAME - Based on Type parameter
            // =====================================

            string tableName = "";
            //BASED ON EMPCATEGORY
            if (empCategory == "OFFICER")
            {
                switch (type)
                {
                    case "INVESTMENT":
                        tableName = "TaxCal"; // Replace with actual table name
                        reportName = "Investment";
                        break;
                    case "ASSESSMENT":
                        tableName = "TaxCal"; // Replace with actual table name
                        reportName = "TaxCalculation_officeR";
                        break;
                    case "CHALLAN":
                        tableName = "IncomeCertificate"; // Replace with actual table name
                        reportName = "IncomeTaxCer_details";
                        break;
                    default:
                        throw new Exception("Invalid Type parameter");
                }

            }
            else if(empCategory == "STAFF")
            {
                switch (type)
                {
                    case "INVESTMENT":
                        tableName = "TaxCal_Staff"; // Replace with actual table name
                        reportName = "Investment";
                        break;
                    case "ASSESSMENT":
                        tableName = "TaxCal_Staff"; // Replace with actual table name
                        reportName = "TaxCalculation_officeR";
                        break;
                    case "CHALLAN":
                        tableName = "IncomeCertificate_Staff"; // Replace with actual table name
                        reportName = "IncomeTaxCer_details";
                        break;
                    default:
                        throw new Exception("Invalid Type parameter");
                }
            }
            // Map Type parameter to appropriate table name
     

            // Use reportName from querystring or default
            string reportFileName = string.IsNullOrEmpty(reportName) ? "TaxCalculation_officeR" : reportName;

            // =====================================
            // QUERY - Use actual table name from mapping
            // =====================================

            string query = $@"
        SELECT *
        FROM [{tableName}]
        WHERE (@empno IS NULL OR empno = @empno) ORDER BY DEPTCODE";

            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@empno",
                    string.IsNullOrEmpty(empno) ? (object)DBNull.Value : empno);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new Exception("No data found");

            // =====================================
            // REPORT LOAD - Use reportFileName
            // =====================================

            string reportPath =
                Server.MapPath($"~/{dbName}/{reportFileName}.rpt");

            if (!System.IO.File.Exists(reportPath))
                throw new Exception($"Report file not found: {reportPath}");

            crp = new ReportDocument();
            crp.Load(reportPath);
            crp.SetDataSource(ds.Tables[0]);

            // =====================================
            // EXPORT PDF
            // =====================================

            Response.Clear();
            Response.Buffer = false;

            // Generate appropriate filename based on type
           

        crp.ExportToHttpResponse(
            ExportFormatType.PortableDocFormat,
            Response,
            false,
            "aaa"
        );
    }
    catch (Exception ex)
    {
        Response.Clear();
        Response.ContentType = "text/html";
        Response.Write($"<h3>Tax Report Error</h3>{ex.Message}");
    }
    finally
    {
        if (crp != null)
        {
            crp.Close();
            crp.Dispose();
        }
    }
}
}
