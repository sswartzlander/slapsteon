<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Slapsteon.Web.Models.DeviceList>" %>

<%@ Import Namespace="Slapsteon.Web.Helpers" %>
<table cellpadding="10">
<% Html.Repeater(Model.Devices, device =>
   { %>
   <tr>
       <td> <h3><%= device.Name %></h3> </td>
       <td> <h3><%= device.Status %></h3></td>
       <td> <h3><%: Html.ActionLink("On", "On", device) %></h3></td>
       <td> <h3><%: Html.ActionLink("Off", "Off", device) %></h3></td>
       <td> <%= "LastOn:" +device.LastOn %></td>
       <td> <%= "LastOff: "+ device.LastOff %></td>
   </tr>
<% }); %>
</table>