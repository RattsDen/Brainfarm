<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="TempDevelopingSearchPage.aspx.cs" Inherits="BrainfarmWeb.TempDevelopingSearchPage" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">
    <div class="panel">
        <p>
            Form Panel:
        </p>
        <div class="panel">
            <asp:TextBox runat="server" ID="txtProjectTags" placeholder="Enter Project Tags here, separated by spaces.."
                                    CssClass="text-field">
            </asp:TextBox>

            <asp:Button runat="server" ID="btnSeparateTags" Text="Debug: Separate Tags"
                        CssClass="btn" OnClick="btnSeparateTags_Click">
            </asp:Button>
 
        </div>
        
        <p>
            Debug Results panel:
        </p>
        <div class="panel">
            <asp:Label runat="server" ID="debugText">
            </asp:Label>
        </div>
                
        
    </div>

</asp:Content>
