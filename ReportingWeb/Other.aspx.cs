using System;
using System.Data;
using System.Data.SqlClient;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class Other : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string action = Request.QueryString["action"];

            if (!string.IsNullOrEmpty(action))
            {
                switch (action)
                {
                    case "EMP":
                        GenerateEmployeeReport();
                        break;
                    case "PFSTATEMENT":
                        GeneratePFStatement();
                        break;
                    case "LOCALHBLOAN":
                        GenerateLocalHBLoan();
                        break;
                    case "HBLOAN":
                        GenerateHBLoanBCIC();
                        break;
                    case "MCLOAN":
                        GenerateMCLoanBCIC();
                        break;
                    case "PFLOAN":
                        GeneratePFLoan();
                        break;
                    case "WFLOAN":
                        GenerateWFLoanLocal();
                        break;
                    case "WFLOANOTHER":
                        GenerateWFLoanOther();
                        break;
                    case "FC":
                        GenerateFCReport();
                        break;
                    case "OVERTIME":
                        GenerateOvertimeReport();
                        break;
                    case "FESTIVAL":
                        GenerateFestivalReport();
                        break;
                    case "BAISAKHI":
                        GenerateBaisakhiReport();
                        break;
                    case "bankst":
                        GenerateBankSt();
                        break;
                    default:
                        Response.Write("<h3>Error</h3>Invalid action specified");
                        break;
                }
            }
            else
            {
                Response.Write("<h3>Error</h3>No action specified");
            }
        }
    }

    #region Employee Report Methods

    private void GenerateEmployeeReport()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = "";
            if (category== "OFFICER")
            {
                tableName = "EmpOfficer_Pay";
            }
            else if(category == "STAFF")
            {
                tableName = "EmpStaff_Pay";
            }
            else if (category == "NPS")
            {
                tableName = "EmpWorkerNps_Pay";
            }
            else if (category == "WAGES")
            {
                tableName = "EmpWorkerWages_Pay";
            }
            // string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT * 
                FROM [{tableName}]
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No data found for the selected category");

            crp = LoadReport(dbName, "EmployeeList", ds);
            ExportReport(crp, "EmployeeList");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GeneratePFStatement()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, PFAMNT, PFADV, TOTPF 
                FROM [{tableName}]
                WHERE PFAMNT > 0 OR PFADV > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No PF data found for the selected category");

            crp = LoadReport(dbName, "PFStatement", ds);
            ExportReport(crp, "PFStatement");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    #endregion

    #region Loan Report Methods

    private void GenerateLocalHBLoan()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, HBLOCAL, HBLOCALADV 
                FROM [{tableName}]
                WHERE HBLOCAL > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No Local HB Loan data found for the selected category");

            crp = LoadReport(dbName, "LocalHBLoan", ds);
            ExportReport(crp, "LocalHBLoan");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateHBLoanBCIC()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, HBAMNT, HBADV 
                FROM [{tableName}]
                WHERE HBAMNT > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No HB Loan (BCIC) data found for the selected category");

            crp = LoadReport(dbName, "HBLoanBCIC", ds);
            ExportReport(crp, "HBLoanBCIC");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateMCLoanBCIC()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, MCLOAN, MCLOANADV 
                FROM [{tableName}]
                WHERE MCLOAN > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No M.C Loan (BCIC) data found for the selected category");

            crp = LoadReport(dbName, "MCLoanBCIC", ds);
            ExportReport(crp, "MCLoanBCIC");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GeneratePFLoan()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, PFLOAN, PFLOANADV 
                FROM [{tableName}]
                WHERE PFLOAN > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No P.F Loan data found for the selected category");

            crp = LoadReport(dbName, "PFLoan", ds);
            ExportReport(crp, "PFLoan");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateWFLoanLocal()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, WFLOCAL, WFLOCALADV 
                FROM [{tableName}]
                WHERE WFLOCAL > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No W.F Loan (Local) data found for the selected category");

            crp = LoadReport(dbName, "WFLoanLocal", ds);
            ExportReport(crp, "WFLoanLocal");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateWFLoanOther()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(category, month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, WFOTHER, WFOTHERADV 
                FROM [{tableName}]
                WHERE WFOTHER > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No W.F Loan (Other) data found for the selected category");

            crp = LoadReport(dbName, "WFLoanOther", ds);
            ExportReport(crp, "WFLoanOther");
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    #endregion

    #region Special Report Methods

    private void GenerateFCReport()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string reportName = GetSpecialReportName("FC", category);

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName("OFFICER", month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, FCAMNT, FCADV 
                FROM [{tableName}]
                WHERE FCAMNT > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No FC data found");

            crp = LoadReport(dbName, reportName, ds);
            ExportReport(crp, reportName);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateOvertimeReport()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string reportName = GetSpecialReportName("OVERTIME", category);

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName("OFFICER", month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, OTHOUR, OTAMNT 
                FROM [{tableName}]
                WHERE OTAMNT > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No Overtime data found");

            crp = LoadReport(dbName, reportName, ds);
            ExportReport(crp, reportName);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateFestivalReport()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string reportName = GetSpecialReportName("FESTIVAL", category);

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName("OFFICER", month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, FESTAMNT, FESTADV 
                FROM [{tableName}]
                WHERE FESTAMNT > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No Festival data found");

            crp = LoadReport(dbName, reportName, ds);
            ExportReport(crp, reportName);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    private void GenerateBaisakhiReport()
    {
        ReportDocument crp = null;

        try
        {
            string dbName = Request.QueryString["key"];
            string category = Request.QueryString["category"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string reportName = GetSpecialReportName("BAISAKHI", category);

            ValidateParameters(dbName, category);

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName("OFFICER", month, year);

            string query = $@"
                SELECT EMPNO, EMPNAME, BAISAKHI, BAISAKHIADV 
                FROM [{tableName}]
                WHERE BAISAKHI > 0
                ORDER BY EMPNO";

            DataSet ds = GetData(connectionString, query);

            if (ds.Tables[0].Rows.Count == 0)
                throw new Exception("No Baisakhi data found");

            crp = LoadReport(dbName, reportName, ds);
            ExportReport(crp, reportName);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    #endregion

    #region Bank Statement Method

    private void GenerateBankSt()
    {
        ReportDocument crp = null;

        try
        {
            string bankName = Request.QueryString["bankName"];
            string month = Request.QueryString["mnt"];
            string year = Request.QueryString["year"];
            string dbName = Request.QueryString["key"];
            string reportName = Request.QueryString["reportName"];
            string empCategory = Request.QueryString["empCategory"];

            if (string.IsNullOrEmpty(reportName))
                reportName = "BankStatement";

            ValidateParameters(dbName, empCategory);

            if (string.IsNullOrEmpty(bankName))
                throw new Exception("Bank name is required");

            string connectionString = GetConnectionString(dbName);
            string tableName = GetTableName(empCategory, month, year);

            string query = $@"
                SELECT *
                FROM [{tableName}]
                WHERE BNKNAM = @bankName";

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
                throw new Exception($"No data found for bank: {bankName}");

            crp = LoadReport(dbName, reportName, ds);
            ExportReport(crp, reportName);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
        finally
        {
            DisposeReport(crp);
        }
    }

    #endregion

    #region Helper Methods

    private string GetConnectionString(string dbName)
    {
        return $"Server=103.7.112.190,1433;" +
               $"Database={dbName};" +
               $"User Id=myuser;" +
               $"Password=1234;" +
               $"TrustServerCertificate=True;";
    }

    private void ValidateParameters(string dbName, string category)
    {
        if (string.IsNullOrEmpty(dbName))
            throw new Exception("Database name is missing");

        if (string.IsNullOrEmpty(category))
            throw new Exception("Category is missing");
    }

    private string GetTableName(string category, string month, string year)
    {
        // If month and year are provided, use the monthly table
        if (!string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
        {
            return $"Master{category}_Pay_{month}_{year}";
        }

        // Otherwise use the main table
        return $"Master{category}_Pay";
    }

    private DataSet GetData(string connectionString, string query)
    {
        DataSet ds = new DataSet();
        using (SqlConnection con = new SqlConnection(connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                sda.Fill(ds);
            }
        }
        return ds;
    }

    private ReportDocument LoadReport(string dbName, string reportName, DataSet ds)
    {
        ReportDocument crp = new ReportDocument();
        string reportPath = Server.MapPath($"~/{dbName}/{reportName}.rpt");

        if (!System.IO.File.Exists(reportPath))
            throw new Exception($"Report file not found: {reportName}.rpt");

        crp.Load(reportPath);
        crp.SetDataSource(ds.Tables[0]);
        return crp;
    }

    private void ExportReport(ReportDocument crp, string fileName)
    {
        Response.Clear();
        Response.Buffer = false;
        Response.ContentType = "application/pdf";
        crp.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, fileName);
        Response.End();
    }

    private void HandleError(Exception ex)
    {
        Response.Clear();
        Response.ContentType = "text/html";
        Response.Write($@"
            <html>
            <head><title>Error</title></head>
            <body>
                <h3 style='color:red;'>Error Generating Report</h3>
                <p><strong>Message:</strong> {ex.Message}</p>
                <p><strong>Details:</strong> {ex.StackTrace}</p>
            </body>
            </html>
        ");
    }

    private void DisposeReport(ReportDocument crp)
    {
        if (crp != null)
        {
            try
            {
                crp.Close();
                crp.Dispose();
            }
            catch { /* Ignore disposal errors */ }
        }
    }

    #endregion

    #region Report Name Mappers

    private string GetSpecialReportName(string reportType, string category)
    {
        string reportName = reportType;

        switch (category.ToUpper())
        {
            case "SUMMARY":
                reportName += "Summary";
                break;
            case "BANKSTATEMENT":
                reportName += "BankStatement";
                break;
            case "BANKLETTER":
                reportName += "BankLetter";
                break;
            default:
                reportName += "Summary";
                break;
        }

        return reportName;
    }

    #endregion
}