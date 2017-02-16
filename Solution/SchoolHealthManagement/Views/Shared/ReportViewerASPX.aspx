<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<%@ Register assembly="Microsoft.ReportViewer.WebForms, Version=11.0.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91" namespace="Microsoft.Reporting.WebForms" tagprefix="rsweb" %>

<!DOCTYPE html>
<script runat="server">

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            using (SchoolHealthManagement.Controllers.HomeController hm = new SchoolHealthManagement.Controllers.HomeController())
            {
                        
                RptViewerCommon.LocalReport.ReportPath = Server.MapPath("~/Reports/ListMonitoringOfficers.rdlc");
                ReportDataSource ds = new ReportDataSource("DataSet4Rpts", (System.Data.DataTable)hm.getMonitoringOfficerInfo());
                RptViewerCommon.LocalReport.DataSources.Clear();
                RptViewerCommon.LocalReport.DataSources.Add(ds);
                RptViewerCommon.LocalReport.Refresh();

            }
        }
    }
</script>


<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>ReportViewerASPX</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <rsweb:ReportViewer ID="RptViewerCommon" runat="server" AsyncRendering="false" SizeToReportContent="true">
        </rsweb:ReportViewer>
        
    </div>
    </form>
</body>
</html>
