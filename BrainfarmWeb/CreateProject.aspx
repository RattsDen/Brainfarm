<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="CreateProject.aspx.cs" Inherits="BrainfarmWeb.CreatePage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <link rel="stylesheet" href="/styles/CreateProject.css"/>
    <link rel="stylesheet" href="/styles/Project.css"/>  <!-- for CSS for comments -->    

</asp:Content>
     
<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">


    <div class="panel">
        <asp:TextBox runat="server" ID="txtProjectTitle" placeholder="Enter Project Title here.."
                                    CssClass="text-field text-field-big">
        </asp:TextBox>

        <asp:TextBox runat="server" ID="txtProjectTags" placeholder="Enter Project Tags here, separated by spaces.."
                                    CssClass="text-field">
        </asp:TextBox>

    </div>

    <div class="panel">
        <div class="comment">
            <div class="commentHead">
                <span class="username">Create New Project</span>
            </div>
            <div class="ribbon initial">PROJECT</div>
        

            <asp:TextBox runat="server" ID="txtCreateProjectDescription"
                         placeholder="Type a description of your project here..."
                         CssClass="text-field commentContent">

            </asp:TextBox>
        </div>
    </div>

    <div class="panel">
        <asp:Button runat="server" ID="btnCreateNewProject" Text="CREATE NEW PROJECT"
                    CssClass="btn" OnClick="btnCreateNewProject_Click">
        </asp:Button>

        <asp:Label runat="server" ID="lblError" Visible="false" CssClass="error-message">
        </asp:Label>

    </div>

    <div class="panel">
        <p>This is debug text.</p>
        <asp:Label runat="server" ID="debugText"></asp:Label>
    </div>
    
</asp:Content>
