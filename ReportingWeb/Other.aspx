<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeFile="Other.aspx.cs" Inherits="Other" %>

<%@ Register assembly="CrystalDecisions.Web, Version=13.0.3500.0, Culture=neutral, PublicKeyToken=692fbea5521e1304" namespace="CrystalDecisions.Web" tagprefix="CR" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
       
    </div>
    <br/>
    <div>
        
   
        <CR:CrystalReportViewer ID="CrystalReportViewer1" runat="server" AutoDataBind="true" />
        <br />
    </div>
    
</asp:Content>
