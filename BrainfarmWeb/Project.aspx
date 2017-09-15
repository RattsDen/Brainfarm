<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Project.aspx.cs" Inherits="BrainfarmWeb.Project" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="/styles/Project.css"/>

    <script>
        // ID of project from request
        var projectID = parseInt(<%= projectID %>);
    </script>
    
    <script src="/plugins/handlebars-v4.0.10.js"></script>
    <script src="/scripts/ServiceAjax.js"></script>
    <script src="/scripts/Project.js"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">
    <div id="div-project-title" class="panel">
        <h1>
            <asp:Label runat="server" ID="lblProjectTitle"></asp:Label>
        </h1>
        <asp:Panel runat="server" ID="panelProjectTags">
        </asp:Panel>
    </div>

    <div id="div-project-comments" class="panel">
        <asp:TextBox runat="server" ID="txtComment" TextMode="MultiLine" Rows="4" Width="35%" />
        <div>
            <asp:CheckBox runat="server" ID="chkIsSpecification" Text="Specification Comment"/>
        </div>
        <div>
            <asp:CheckBox runat="server" ID="chkIsSynthesis" Text="Synthesis Comment"/>
        </div>
        <div>
            <asp:CheckBox runat="server" ID="chkIsContribution" Text="Contribution Comment"/>
        </div>
        <asp:Button runat="server" ID="btnSubmitComment" Text="Submit Comment" CssClass="btn" OnClick="btnSubmitComment_Click"/>

    </div>

</asp:Content>
