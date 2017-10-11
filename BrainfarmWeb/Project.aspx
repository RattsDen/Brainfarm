<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Project.aspx.cs" Inherits="BrainfarmWeb.Project" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="/styles/Project.css"/>

    <script>
        // ID of project from request
        var projectID = parseInt(<%= projectID %>);
        var sessionToken = "<%= sessionToken%>";
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
    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContentPlaceHolder" runat="server">
    <div>
        <h4 class="sidebar-header">Filter Comments</h4>
        Show only:
        <div id="div-filter-options">
            <label><input type="checkbox" id="chk-filter-normal" />Normal Comments</label>
            <label><input type="checkbox" id="chk-filter-synth" />Synthesis</label>
            <label><input type="checkbox" id="chk-filter-spec" />Specification</label>
            <label><input type="checkbox" id="chk-filter-contrib" />Contribution</label>
            <label><input type="checkbox" id="chk-filter-bookmarked" />Bookmarked</label>
        </div>
        <div id="div-filter-buttons">
            <a href="javascript:;" id="btn-filter-apply" class="btn">Apply Filter</a>
            <a href="javascript:;" id="btn-filter-remove" class="btn">Remove Filter</a>
        </div>
    </div>
</asp:Content>
