<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TraceabilityMatrix.aspx.cs" Inherits="TraceAbiltyMatrix.TraceabilityMatrix" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Traceability Matrix</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        body {
            padding: 40px;
            background-color: #f8f9fa;
        }
        .container {
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }
        .form-label {
            font-weight: bold;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2 class="mb-4 text-center">Traceability Matrix</h2>

            <%--<div class="row mb-3">
                <div class="col-md-4 offset-md-2">
                    <asp:Label ID="lblEnterSerial" runat="server" Text="Enter Serial Number:" CssClass="form-label"></asp:Label>
                </div>
                <div class="col-md-4">
                    <asp:TextBox ID="txtSerialNumber" runat="server" CssClass="form-control" />
                </div>
            </div>

            <div class="row mb-4 text-center">
                <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn btn-primary" />
            </div>--%>


            <div class="row mb-4 align-items-end">
    <div class="col-md-4 offset-md-2">
        <asp:Label ID="lblEnterSerial" runat="server" Text="Enter Serial Number:" CssClass="form-label"></asp:Label>
        <asp:TextBox ID="txtSerialNumber" runat="server" CssClass="form-control" />
    </div>
    <div class="col-md-2">
        <asp:Button ID="btnSearch" runat="server" Text="Search" OnClick="btnSearch_Click" CssClass="btn btn-primary w-100" />
    </div>
</div>


            <div class="row">
                <div class="col-md-12">
                    <asp:GridView ID="gvTraceability" runat="server" AutoGenerateColumns="true"
                        EmptyDataText="No matching traceability data found."
                        CssClass="table table-bordered table-striped table-hover">
                    </asp:GridView>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
