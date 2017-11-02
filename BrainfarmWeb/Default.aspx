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
                Brainfarm is a reddit-style board for discussing project ideas, 
                and to motivate people who are good at building things to practice 
                their building skills! Brainfarm has special features to help users 
                gather the best ideas that appear in discussion, and to help 
                builders find challenges to build and showcase their work.
            </p>
            <p>
                <a href="/About.aspx" class="btn">How do I use this site?</a>
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
