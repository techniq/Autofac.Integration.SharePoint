<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Import Namespace="Microsoft.SharePoint.ApplicationPages" %>
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=14.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Page Language="C#" Async="true" AutoEventWireup="true" CodeBehind="Play.aspx.cs" Inherits="Autofac.Integration.SharePoint.Demo.Layouts.AutofacPlayground.Play" DynamicMasterPageFile="~masterurl/default.master" %>

<asp:Content ID="PageHead" ContentPlaceHolderID="PlaceHolderAdditionalPageHead" runat="server">

</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="PlaceHolderMain" runat="server">

        <p>
         <asp:Label ID="lblMessage" runat="server" Text=""></asp:Label>
        </p>
        <p>
            <asp:Button ID="btnTest1" runat="server" Text="Postback test with property injection" onclick="DoTest1" />
        </p>
        <p>
            <asp:Button ID="btnTest2" runat="server" Text="Postback test with service locator" onclick="DoTest2" />
        </p>
        <p>
            <asp:Button ID="btnTest3" runat="server" Text="Test with elevated privileges" onclick="DoTest3" />
        </p>
        <p>
            <asp:Button ID="btnTest4" runat="server" Text="Test where HttpContext.Current is null" onclick="DoTest4" />
        </p>
        <p>
            <asp:Button ID="btnTest5" runat="server" Text="Test in Background Thread" onclick="DoTest5" />
        </p>
        <p>
        <asp:Literal ID="litComments" runat="server"></asp:Literal>
        </p>
</asp:Content>

<asp:Content ID="PageTitle" ContentPlaceHolderID="PlaceHolderPageTitle" runat="server">
Application Page
</asp:Content>


<asp:Content ID="PageTitleInTitleArea" ContentPlaceHolderID="PlaceHolderPageTitleInTitleArea" runat="server" >
My Application Page
</asp:Content>
