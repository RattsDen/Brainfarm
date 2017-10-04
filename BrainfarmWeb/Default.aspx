<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="BrainfarmWeb.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link rel="stylesheet" href="/styles/Dashboard.css"/>
    <link rel="stylesheet" href="/styles/ProjectList.css"/>

    <script src="/scripts/Dashboard.js"></script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">
    
    <asp:Panel runat="server" ID="panelLoggedOut" Visible="true">
        <div class="panel">
            <h2>Welcome to Brainfarm!</h2>
            <p>
                Lorem ipsum dolor sit amet, consectetur adipiscing elit. 
                Nulla rhoncus leo vel lacinia eleifend. Nullam nisl risus, viverra et diam in, 
                efficitur scelerisque neque. Vivamus vel augue turpis. Fusce consequat libero 
                ligula, id hendrerit felis ornare ut. Pellentesque tempor enim leo. 
                Sed a tellus ac lorem semper interdum a bibendum ante. Donec vitae sapien 
                id urna lacinia rutrum. Sed consectetur metus eu molestie tincidunt.
            </p>
            <p>
                <a href="javascript:;" class="btn">How do I use this site?</a>
                <a href="/Register.aspx" class="btn">Register a new account</a>
            </p>
        </div>
    </asp:Panel>

    <asp:Panel runat="server" ID="panelLoggedIn" Visible="false">
        <div class="panel">
            <h2>Welcome, <asp:Label runat="server" ID="lblUsername"></asp:Label></h2>
        </div>
    </asp:Panel>

    <div id="div-dashboard">

        <asp:Panel runat="server" ID="panelPopular" CssClass="panel div-dashboard-column">
            <h3>Popular Projects</h3>
        </asp:Panel>

        <asp:Panel runat="server" ID="panelRecommended" CssClass="panel div-dashboard-column">
            <h3>Recommended Projects</h3>
        </asp:Panel>

        <asp:Panel runat="server" ID="panelBookmarks" CssClass="panel div-dashboard-column">
            <h3>Your Bookmarks</h3>
        </asp:Panel>

    </div>
    
</asp:Content>
