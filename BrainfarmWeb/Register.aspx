<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="BrainfarmWeb.Register" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="/styles/Register.css"/>
    <script src="/scripts/ServiceAjax.js"></script>
    <script src="/scripts/Register.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">

    <div id="div-register" class="panel">
        <h1>Register New Account</h1>

        <asp:Label runat="server" ID="lblError" Visible="false" CssClass="error-message"></asp:Label>

        <table>
            <tr><td>
                <asp:TextBox runat="server" ID="txtUsername" placeholder="Username" CssClass="text-field"></asp:TextBox>
            </td></tr>
            <tr><td>
                <asp:TextBox runat="server" ID="txtNewPassword" TextMode="Password" placeholder="Password" CssClass="text-field"></asp:TextBox>
            </td></tr>
            <tr><td>
                <asp:TextBox runat="server" ID="txtPasswordConfirm" TextMode="Password" placeholder="Retype Password" CssClass="text-field"></asp:TextBox>
            </td></tr>
            <tr><td>
                <asp:TextBox runat="server" ID="txtEmail" placeholder="Email Address" CssClass="text-field"></asp:TextBox>
            </td></tr>
            <tr><td>
                <asp:Button runat="server" ID="btnRegister" Text="Register" OnClick="btnRegister_Click" CssClass="btn btn-wide" />
            </td></tr>
        </table>
    </div>

</asp:Content>
