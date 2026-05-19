<%@ Page Title="" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true" CodeFile="Worker.aspx.cs" Inherits="Worker" %>
<%@ Register TagPrefix="CR" Namespace="CrystalDecisions.Web" Assembly="CrystalDecisions.Web, Version=13.0.3500.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <div class="jumbotron">
        <h1>WORKER PAY-SLIP</h1>
    </div>
    <br/>
    <div>
        <asp:Button ID="Button1" runat="server" Text="Load Report" class="btn btn-danger" OnClick="Button1_Click" />
        <br />
        <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="true" />
        <br />
    </div>
</asp:Content>

