<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WeiXinApi.aspx.cs" Inherits="WebWeiXinToGZH.WeiXinApi" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server" defaultbutton="btnSend">
    <div>
        <b>request:</b>
        <br />
        <asp:TextBox ID="txtMessage" runat="server" Width="429px" ></asp:TextBox>
        <asp:Button ID="btnSend" runat="server" Text="send" OnClick="btnSend_Click" BackColor="#FF9933"  />

        <br />
        <asp:Label ID="lblMessage" runat="server" Text="" BackColor="#66ccff"></asp:Label>
        <br />
        <br />
        <b>response:</b>
        <br />
        <asp:TextBox ID="txtResult" runat="server" Height="255px" Width="860px" TextMode="MultiLine" Rows="5"></asp:TextBox>
        <br />
    </div>
    </form>
</body>
</html>
