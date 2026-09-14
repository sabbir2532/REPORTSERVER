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
            else if (Request.QueryString["action"] == "CCSum")
            {
                GenerateCCSUM();
            }
            else if (Request.QueryString["action"] == "PaySum")
            {
                GeneratePaySum();
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
            order by ACNO
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
            string orderby = "DEPTCODE";
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
                        orderby = "Flag";
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
                        if (dbName == "JFCL_PAY")
                        {
                            reportName = "TaxCalculation_StaffR";
                        }else
                        reportName = "TaxCalculation_officeR";
                        break;
                    case "CHALLAN":
                        tableName = "IncomeCertificate_Staff"; // Replace with actual table name
                        reportName = "IncomeTaxCer_details";
                        orderby = "Flag";
                        break;
                    default:
                        throw new Exception("Invalid Type parameter");
                }
            }
            else if (empCategory == "JROFFICER")
            {
                switch (type)
                {
                    case "INVESTMENT":
                        tableName = "TaxCal"; // Replace with actual table name
                        reportName = "Investment";
                        break;
                    case "ASSESSMENT":
                        tableName = "TaxCal"; // Replace with actual table name
                        reportName = "TaxCalculation_StaffR";
                        break;
                    case "CHALLAN":
                        tableName = "IncomeCertificate_Staff"; // Replace with actual table name
                        reportName = "IncomeTaxCer_details";
                        orderby = "Flag";
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
            string query = "";
            if (empCategory == "JROFFICER")
            {

                 query = $@"
        SELECT *
        FROM [{tableName}]
        WHERE (@empno IS NULL OR empno = @empno) AND PFPLC=4 ORDER BY {orderby}";
            }
            else {
                query = $@"
        SELECT *
        FROM [{tableName}]
        WHERE (@empno IS NULL OR empno = @empno) ORDER BY {orderby}";

            }

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

    private void GenerateCCSUM()
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
                reportName = "CCSum";

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
                "CCSUMReport"

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

    private void GeneratePaySum()
    {
        ReportDocument crp = null;

        try
        {
            // =====================================
            // PARAMETERS
            // =====================================
            string empCategory = Request.QueryString["empCategory"];
            string mnt = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];

            if (string.IsNullOrEmpty(reportName))
                reportName = "PaySummaryOfficer";

            if (string.IsNullOrEmpty(dbName))
                throw new Exception("Database missing");

            if (string.IsNullOrEmpty(empCategory))
                throw new Exception("Emp Category missing");

            if (string.IsNullOrEmpty(mnt) || string.IsNullOrEmpty(year))
                throw new Exception("Month/Year missing");

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
            // TABLE NAME (whitelisted)
            // =====================================
            string tableName;

            switch (empCategory.ToUpper())
            {
                case "OFFICER": tableName = $"MasterOfficer_Pay_{mnt}_{year}"; break;
                case "STAFF": tableName = $"MasterStaff_Pay_{mnt}_{year}"; break;
                case "NPS": tableName = $"MasterWorkerNps_Pay_{mnt}_{year}"; break;
                case "WAGES": tableName = $"MasterWorkerWages_Pay_{mnt}_{year}"; break;
                default: throw new Exception("Invalid Category");
            }

            // Basic sanity check on the built identifier
            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    tableName, @"^[A-Za-z0-9_]+$"))
                throw new Exception("Invalid table name");

            // =====================================
            // QUERY  -- one row of column-wise SUMs
            // =====================================
            string query = $@"
        SELECT
            /* ---------------- EARNINGS ---------------- */
            SUM(ISNULL(BASIC_EARN,0))            AS BASIC_EARN,
            SUM(ISNULL(HOUSERENTALW_AUTO,0))     AS HOUSERENTALW_AUTO,
            SUM(ISNULL(ARBAS,0))                 AS ARBAS,
            SUM(ISNULL(ARBON,0))                 AS ARBON,
            SUM(ISNULL(ARHR,0))                  AS ARHR,
            SUM(ISNULL(ARTIFALW,0))              AS ARTIFALW,
            SUM(ISNULL(ARMEDALW,0))              AS ARMEDALW,
            SUM(ISNULL(ARGASALW,0))              AS ARGASALW,
            SUM(ISNULL(ARWASALW,0))              AS ARWASALW,
            SUM(ISNULL(ARSFTALW,0))              AS ARSFTALW,
            SUM(ISNULL(ARTRNSALW,0))             AS ARTRNSALW,
            SUM(ISNULL(ARTELALW,0))              AS ARTELALW,
            SUM(ISNULL(CanteenAlw,0))            AS CanteenAlw,
            SUM(ISNULL(MEDALW,0))                AS MEDALW,
            SUM(ISNULL(TIFALW,0))                AS TIFALW,
            SUM(ISNULL(GASALW,0))                AS GASALW,
            SUM(ISNULL(WASALW,0))                AS WASALW,
            SUM(ISNULL(SBENEFITALW,0))           AS SBENEFITALW,
            SUM(ISNULL(CONALW,0))                AS CONALW,
            SUM(ISNULL(CHRALW,0))                AS CHRALW,
            SUM(ISNULL(SFTALW,0))                AS SFTALW,
            SUM(ISNULL(TELEALW,0))               AS TELEALW,
            SUM(ISNULL(HILLALW,0))               AS HILLALW,
            SUM(ISNULL(HONOR,0))                 AS HONOR,
            SUM(ISNULL(EDUALW,0))                AS EDUALW,
            SUM(ISNULL(RISKALW,0))               AS RISKALW,
            SUM(ISNULL(NIGHTSHIFTALW,0))         AS NIGHTSHIFTALW,
            SUM(ISNULL(MISADD,0))                AS MISADD,

            /* ---------------- DEDUCTIONS ---------------- */
            SUM(ISNULL(APFDED,0))                AS APFDED,
            SUM(ISNULL(AAPFDED,0))               AS AAPFDED,
            SUM(ISNULL(AHRDED,0))                AS AHRDED,
            SUM(ISNULL(MEDDED,0))                AS MEDDED,
            SUM(ISNULL(TRNDED,0))                AS TRNDED,
            SUM(ISNULL(TELDED,0))                AS TELDED,
            SUM(ISNULL(LHBDED,0))                AS LHBDED,
            SUM(ISNULL(SALDED,0))                AS SALDED,
            SUM(ISNULL(INCOMETAXDED,0))          AS INCOMETAXDED,
            SUM(ISNULL(MCDED,0))                 AS MCDED,
            SUM(ISNULL(HBDED,0))                 AS HBDED,
            SUM(ISNULL(PFDED,0))                 AS PFDED,
            SUM(ISNULL(WFDED,0))                 AS WFDED,
            SUM(ISNULL(LHB_INTEREST_DED,0))      AS LHB_INTEREST_DED,
            SUM(ISNULL(LCWFDD,0))                AS LCWFDD,
            SUM(ISNULL(FACILITYDED,0))           AS FACILITYDED,
            SUM(ISNULL(SCHOOLDED,0))             AS SCHOOLDED,
            SUM(ISNULL(ELECTCHRG,0))             AS ELECTCHRG,
            SUM(ISNULL(GASCHRG,0))               AS GASCHRG,
            SUM(ISNULL(FURCHRG,0))               AS FURCHRG,
            SUM(ISNULL(HAZCHRG,0))               AS HAZCHRG,
            SUM(ISNULL(OFFCLBCHRG,0))            AS OFFCLBCHRG,
            SUM(ISNULL(EMPCLBCHRG,0))            AS EMPCLBCHRG,
            SUM(ISNULL(WFCHRG,0))                AS WFCHRG,
            SUM(ISNULL(LADCLBCHRG,0))            AS LADCLBCHRG,
            SUM(ISNULL(SANATANCHRG,0))           AS SANATANCHRG,
            SUM(ISNULL(MOSQUECHRG,0))            AS MOSQUECHRG,
            SUM(ISNULL(DON_COM_CHRG,0))          AS DON_COM_CHRG,
            SUM(ISNULL(DON_INDI_CHRG,0))         AS DON_INDI_CHRG,
            SUM(ISNULL(CHEM_SCTY_CHRG,0))        AS CHEM_SCTY_CHRG,
            SUM(ISNULL(DIPLOCHRG,0))             AS DIPLOCHRG,
            SUM(ISNULL(ENGGCHRG,0))              AS ENGGCHRG,
            SUM(ISNULL(HRCHRG,0))                AS HRCHRG,
            SUM(ISNULL(DISHCHRG,0))              AS DISHCHRG,
            SUM(ISNULL(CBACHRG,0))               AS CBACHRG,
            SUM(ISNULL(HIBICHRG,0))              AS HIBICHRG,
            SUM(ISNULL(REVDED,0))                AS REVDED,
            SUM(ISNULL(PFCONTRI_OWN_AUTO,0))     AS PFCONTRI_OWN_AUTO,
            SUM(ISNULL(ADDIPFCONTRI_OWN_AUTO,0)) AS ADDIPFCONTRI_OWN_AUTO,
            SUM(ISNULL(HOUSERENT_DED,0))         AS HOUSERENT_DED,

            /* ---------------- COMPUTED TOTALS ---------------- */
            SUM(
                ISNULL(BASIC_EARN,0)
              + ISNULL(HOUSERENTALW_AUTO,0)
              + ISNULL(ARBAS,0)          + ISNULL(ARBON,0)
              + ISNULL(ARHR,0)           + ISNULL(ARTIFALW,0)
              + ISNULL(ARMEDALW,0)       + ISNULL(ARGASALW,0)
              + ISNULL(ARWASALW,0)       + ISNULL(ARSFTALW,0)
              + ISNULL(ARTRNSALW,0)      + ISNULL(ARTELALW,0)
              + ISNULL(CanteenAlw,0)     + ISNULL(MEDALW,0)
              + ISNULL(TIFALW,0)         + ISNULL(GASALW,0)
              + ISNULL(WASALW,0)         + ISNULL(SBENEFITALW,0)
              + ISNULL(CONALW,0)         + ISNULL(CHRALW,0)
              + ISNULL(SFTALW,0)         + ISNULL(TELEALW,0)
              + ISNULL(HILLALW,0)        + ISNULL(HONOR,0)
              + ISNULL(EDUALW,0)         + ISNULL(RISKALW,0)
              + ISNULL(NIGHTSHIFTALW,0)  + ISNULL(MISADD,0)
            ) AS GROSSPAY,

            SUM(
                ISNULL(APFDED,0)           + ISNULL(AAPFDED,0)
              + ISNULL(AHRDED,0)           + ISNULL(MEDDED,0)
              + ISNULL(TRNDED,0)           + ISNULL(TELDED,0)
              + ISNULL(LHBDED,0)           + ISNULL(SALDED,0)
              + ISNULL(INCOMETAXDED,0)     + ISNULL(MCDED,0)
              + ISNULL(HBDED,0)            + ISNULL(PFDED,0)
              + ISNULL(WFDED,0)            + ISNULL(LHB_INTEREST_DED,0)
              + ISNULL(LCWFDD,0)           + ISNULL(FACILITYDED,0)
              + ISNULL(SCHOOLDED,0)        + ISNULL(ELECTCHRG,0)
              + ISNULL(GASCHRG,0)          + ISNULL(FURCHRG,0)
              + ISNULL(HAZCHRG,0)          + ISNULL(OFFCLBCHRG,0)
              + ISNULL(EMPCLBCHRG,0)       + ISNULL(WFCHRG,0)
              + ISNULL(LADCLBCHRG,0)       + ISNULL(SANATANCHRG,0)
              + ISNULL(MOSQUECHRG,0)       + ISNULL(DON_COM_CHRG,0)
              + ISNULL(DON_INDI_CHRG,0)    + ISNULL(CHEM_SCTY_CHRG,0)
              + ISNULL(DIPLOCHRG,0)        + ISNULL(ENGGCHRG,0)
              + ISNULL(HRCHRG,0)           + ISNULL(DISHCHRG,0)
              + ISNULL(CBACHRG,0)          + ISNULL(REVDED,0)
              + ISNULL(HIBICHRG,0)         + ISNULL(PFCONTRI_OWN_AUTO,0)
              + ISNULL(ADDIPFCONTRI_OWN_AUTO,0)
              + ISNULL(HOUSERENT_DED,0)
            ) AS TOTALDED,

            SUM(
                ISNULL(BASIC_EARN,0)
              + ISNULL(HOUSERENTALW_AUTO,0)
              + ISNULL(ARBAS,0)          + ISNULL(ARBON,0)
              + ISNULL(ARHR,0)           + ISNULL(ARTIFALW,0)
              + ISNULL(ARMEDALW,0)       + ISNULL(ARGASALW,0)
              + ISNULL(ARWASALW,0)       + ISNULL(ARSFTALW,0)
              + ISNULL(ARTRNSALW,0)      + ISNULL(ARTELALW,0)
              + ISNULL(CanteenAlw,0)     + ISNULL(MEDALW,0)
              + ISNULL(TIFALW,0)         + ISNULL(GASALW,0)
              + ISNULL(WASALW,0)         + ISNULL(SBENEFITALW,0)
              + ISNULL(CONALW,0)         + ISNULL(CHRALW,0)
              + ISNULL(SFTALW,0)         + ISNULL(TELEALW,0)
              + ISNULL(HILLALW,0)        + ISNULL(HONOR,0)
              + ISNULL(EDUALW,0)         + ISNULL(RISKALW,0)
              + ISNULL(NIGHTSHIFTALW,0)  + ISNULL(MISADD,0)
            )
            -
            SUM(
                ISNULL(APFDED,0)           + ISNULL(AAPFDED,0)
              + ISNULL(AHRDED,0)           + ISNULL(MEDDED,0)
              + ISNULL(TRNDED,0)           + ISNULL(TELDED,0)
              + ISNULL(LHBDED,0)           + ISNULL(SALDED,0)
              + ISNULL(INCOMETAXDED,0)     + ISNULL(MCDED,0)
              + ISNULL(HBDED,0)            + ISNULL(PFDED,0)
              + ISNULL(WFDED,0)            + ISNULL(LHB_INTEREST_DED,0)
              + ISNULL(LCWFDD,0)           + ISNULL(FACILITYDED,0)
              + ISNULL(SCHOOLDED,0)        + ISNULL(ELECTCHRG,0)
              + ISNULL(GASCHRG,0)          + ISNULL(FURCHRG,0)
              + ISNULL(HAZCHRG,0)          + ISNULL(OFFCLBCHRG,0)
              + ISNULL(EMPCLBCHRG,0)       + ISNULL(WFCHRG,0)
              + ISNULL(LADCLBCHRG,0)       + ISNULL(SANATANCHRG,0)
              + ISNULL(MOSQUECHRG,0)       + ISNULL(DON_COM_CHRG,0)
              + ISNULL(DON_INDI_CHRG,0)    + ISNULL(CHEM_SCTY_CHRG,0)
              + ISNULL(DIPLOCHRG,0)        + ISNULL(ENGGCHRG,0)
              + ISNULL(HRCHRG,0)           + ISNULL(DISHCHRG,0)
              + ISNULL(CBACHRG,0)          + ISNULL(REVDED,0)
              + ISNULL(HIBICHRG,0)         + ISNULL(PFCONTRI_OWN_AUTO,0)
              + ISNULL(ADDIPFCONTRI_OWN_AUTO,0)
              + ISNULL(HOUSERENT_DED,0)
            ) AS NETPAY
        FROM [{tableName}];
        ";

            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(ds);
            }

            if (ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new Exception("No data found");

            // =====================================
            // REPORT LOAD
            // =====================================
            string reportPath = Server.MapPath($"~/{dbName}/PaySummary/{reportName}.rpt");

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
                "CCSUMReport"
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
}
