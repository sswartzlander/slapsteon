<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Slapsteon.Web.Models.Lights>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Index</title>
</head>
<body>
    <div>
    <table>
    <% for (int i=0;i<this.Model.AllLights.Count;i++) { %>
    <tr>
    <td> Name: <%= this.Model.AllLights[i].Name %></td>
    <td> Level: <%= this.Model.AllLights[i].Level %></td>
    <td ><%: Html.ActionLink("On", "On") %></td>
    <td> <%: Html.ActionLink("Off", "Off") %></td>
    </tr>
     <% } %>
     </table>
    </div>

</body>
</html>
