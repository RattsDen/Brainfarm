<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="SearchForProjects.aspx.cs" Inherits="BrainfarmWeb.SearchForProjects" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="/styles/Search.css"/>
    <link rel="stylesheet" href="/styles/ProjectList.css"/>
    <script src="/scripts/SearchForProjects.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">
    
    <div class="panel">

        <h2>Search for Projects</h2>

        <p>
            Enter search words below. Search results will include projects with ALL keywords in any of the selected areas. <br />
            Seperate keywords using spaces. <br />
        </p>

        <div>
            <div id="div-search-bar">
                <asp:TextBox runat="server" id="txtSearchKeywords" placeholder="Enter keywords here.." 
                             CssClass="text-field text-field-big">
                </asp:TextBox>
        
                <asp:Button runat="server" id="btnSearch" Text="Search"
                            CssClass="btn" OnClick="btnSearch_Click">
                </asp:Button>
            </div>

            <asp:Label runat="server" id="lblErrorMessage" CssClass="error-message" Visible="false">
            </asp:Label>
        </div>

        <div>
            <asp:CheckBox runat="server" id="checkboxTags" checked="true"></asp:CheckBox>
            Search for projects with all given keywords in its Tags.
        </div>
        
        <div>
            <asp:CheckBox runat="server" id="checkboxTitles"></asp:CheckBox>
            Search for projects with all given keywords in its Title.
        </div>
        
    
    </div>
    
    <asp:Panel runat="server" ID="resultsPanel" CssClass="panel">
        <h2>Search Results</h2>

     
    </asp:Panel>

 
    
</asp:Content>
