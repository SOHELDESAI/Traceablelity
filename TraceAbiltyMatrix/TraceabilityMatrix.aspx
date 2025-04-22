<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TraceabilityMatrix.aspx.cs" Inherits="TraceAbiltyMatrix.TraceabilityMatrix" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Traceability Matrix</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <h2>Traceability Matrix</h2>

            <asp:Label ID="lblEnterSerial" runat="server" Text="Enter Serial Number:"></asp:Label>
            <asp:TextBox ID="txtSerialNumber" runat="server" Width="200px" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" />

            <br />
            <br />

            <asp:GridView ID="gvTraceability" runat="server" AutoGenerateColumns="true"
                EmptyDataText="No matching traceability data found."
                CssClass="table table-bordered">
            </asp:GridView>
        </div>
    </form>
</body>
</html>
