<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Project.aspx.cs" Inherits="BrainfarmWeb.Project" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="/styles/Project.css"/>

    <script>
        // ID of project from request
        var projectID = parseInt(<%= projectID %>);
    </script>

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
        // Comment section goes here
    </div>

</asp:Content>
