<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Account.aspx.cs" Inherits="BrainfarmWeb.Account" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <link rel="stylesheet" href="/styles/Account.css"/>
    <link rel="stylesheet" href="/styles/Project.css"/>
    <link rel="stylesheet" href="/styles/ProjectList.css"/>

    <script>
        var sessionToken = "<%= sessionToken%>";
        var userID = parseInt(<%= userID %>);
    </script>

    <script src="/plugins/handlebars-v4.0.10.js"></script>
    <script src="/scripts/ServiceAjax.js"></script>
    <script src="/scripts/Account.js"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">

    <div id="div-tabs" class="panel">
        <a href="javascript:;" id="btn-user-projects" class="btn btn-dark">My Projects</a>
        <a href="javascript:;" id="btn-user-comments" class="btn">My Comments</a>
        <a href="javascript:;" id="btn-user-bookmarks" class="btn">My Bookmarks</a>
    </div>

    <div id="div-projects" class="panel tab">
        <h3>My Projects</h3>
        <asp:Panel runat="server" ID="panelProjects">
        </asp:Panel>
    </div>

    <div id="div-comments" class="panel tab hidden-default">
        <h3>My Comments</h3>
        <div id="div-comments-list">
        </div>
    </div>

    <div id="div-bookmarks" class="panel tab hidden-default">
        <h3>My Bookmarks</h3>
        <div id="div-bookmarks-list">
        </div>
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContentPlaceHolder" runat="server">
</asp:Content>
