<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SerialEntry.aspx.cs" Inherits="TraceAbiltyMatrix.SerialEntry" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>BOM Details</title>

    <link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css" rel="stylesheet" />

    <script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js"></script>

    <script type="text/javascript">
        $(document).ready(function () {
            $("#<%= txtBOM.ClientID %>").keyup(function () {
                $.ajax({
                    type: "POST",
                    url: "BOMDetails.aspx/GetBOMSuggestions",
                    data: '{prefix: "' + $(this).val() + '" }',
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    success: function (response) {
                        var data = response.d;
                        var suggestions = "";
                        $.each(data, function (index, value) {
                            suggestions += "<li class='list-group-item' onclick='selectBOM(\"" + value + "\")'>" + value + "</li>";
                        });
                        $("#suggestions").html(suggestions).show();
                    }
                });
            });
        });

        function selectBOM(value) {
            $("#<%= txtBOM.ClientID %>").val(value);
            $("#suggestions").hide();
        }
    </script>

    <style>
        #suggestions {
            list-style: none;
            padding: 0;
            margin-top: 2px;
            border: 1px solid #ccc;
            background: #fff;
            position: absolute;
            width: 100%;
            display: none;
            z-index: 1000;
            max-height: 200px;
            overflow-y: auto;
        }

        #suggestions li {
            padding: 10px;
            cursor: pointer;
        }

        #suggestions li:hover {
            background-color: #f0f0f0;
        }

        .input-group {
            position: relative;
        }

        .fg-serial-group {
            margin-top: 15px;
        }

        .btn-search {
            min-width: 100px;
        }
    </style>
</head>

<body>
    <form id="form1" runat="server">
        <div class="container">

            <div class="row">
                <div class="col-md-6">
                    <label class="control-label">Enter BOM / Description:</label>
                    <div class="input-group">
                        <asp:TextBox ID="txtBOM" runat="server" CssClass="form-control" placeholder="Type BOM..."></asp:TextBox>
                        <div class="input-group-btn">
                            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary btn-search" OnClick="btnSearch_Click" />
                        </div>
                        <ul id="suggestions" class="list-group"></ul>
                    </div>

                    <div class="fg-serial-group">
                        <label class="control-label">Finish Good Serial No:</label>
                        <asp:TextBox ID="txtFGSerial" runat="server" CssClass="form-control" placeholder="Enter FG Serial No."></asp:TextBox>
                    </div>
                </div>
            </div>

            <div class="row" style="margin-top: 30px;">
                <div class="col-md-12">
                    <asp:GridView ID="gvBOM" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped" OnRowDataBound="gvBOM_RowDataBound">
                        <Columns>
                            <asp:BoundField DataField="BOMDesc" HeaderText="BOM Description" />
                            <asp:BoundField DataField="ItemDesc" HeaderText="Item Description" />
                            <asp:BoundField DataField="StockStatus" HeaderText="Stock Status" />
                            <asp:TemplateField HeaderText="Enter Serial Number">
                                <ItemTemplate>
                                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" Visible="false"></asp:TextBox>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>

                    <div class="text-center" style="margin-top: 20px;">
                        <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="btnSubmit_Click" />
                    </div>
                </div>
            </div>

        </div>
    </form>
</body>
</html>
