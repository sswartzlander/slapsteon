using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Insteon.Library
{
    [ServiceContract]
    public interface IInsteonWebService
    {
         [OperationContract]
         [WebGet(UriTemplate = "/Alarm/{x}/{y}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void Alarm(string x, string y);

         [OperationContract]
         [WebGet(UriTemplate = "/Alarm2/{z}/", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void Alarm2(string z);

         [OperationContract]
         [WebGet(UriTemplate = "/Device/{name}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         Device GetDevice(string name);

         [OperationContract]
         [WebGet(UriTemplate = "/Devices", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         Device[] GetDevices();

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void Off(string device);

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/On", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void FastOn(string device);

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/On/{level}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void On(string device, string level);

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/On/{level}/{rate}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void RampOn(string device, string level, string rate);

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/Off/{rate}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void RampOff(string device, string rate);



    }
}
