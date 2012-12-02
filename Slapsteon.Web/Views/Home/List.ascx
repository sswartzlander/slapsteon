<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Slapsteon.Web.Models.DeviceList>" %>

<%@ Import Namespace="Slapsteon.Web.Helpers" %>
<table cellpadding="10">
<% Html.Repeater(Model.Devices, device =>
   { %>
   
   <% if (!device.IsPLM)
      { %>
   <tr>
       <td> <h3><%= device.Name%></h3> </td>
       <td> <h3><%= device.Status%></h3></td>
       <td> <h3><%: Html.ActionLink("On", "On", device)%></h3></td>
       <td> <h3><%: Html.ActionLink("Off", "Off", device)%></h3></td>
       <td> <%= "LastOn:" + device.LastOn.ToShortTimeString()%></td>
       <td> <%= "LastOff: " + device.LastOff.ToShortTimeString()%></td>
   </tr>
   <% if (device.IsDimmable)
      { %>
      <tr>
      <td> <i>brightness</i></td> 
      <td><%:Html.ActionLink("20%", "OnLevel", new { deviceName = device.Name,level= 20}) %></td>
      <td><%:Html.ActionLink("40%", "OnLevel", new { deviceName = device.Name, level = 40 })%></td>
      <td><%:Html.ActionLink("60%", "OnLevel", new { deviceName = device.Name, level = 60 })%></td>
      <td><%:Html.ActionLink("80%", "OnLevel", new { deviceName = device.Name, level = 80 })%></td>
      <td></td>
      </tr>
   <% } %>
   <% if (device.IsFan)
      { %>
      <tr>
      <td><i>fan speed</i></td>
      <td><%:Html.ActionLink("High", "OnFan", new { deviceName = device.Name, level = 100 })%></td>
      <td><%:Html.ActionLink("Medium", "OnFan", new { deviceName = device.Name, level = 66 })%></td>
      <td><%:Html.ActionLink("Low", "OnFan", new { deviceName = device.Name, level = 33 })%></td>
      <td><%:Html.ActionLink("Off", "OnFan", new { deviceName = device.Name, level = 0 })%></td>
      <td></td>
      </tr>
   <% } %>
   <% } %>
<% }); %>
</table>