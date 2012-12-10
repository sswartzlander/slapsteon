﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using Insteon.Devices;

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
         [WebGet(UriTemplate = "/Devices", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
         SlapsteonDevice[] GetDevices();

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

         [OperationContract]
         [WebGet(UriTemplate = "/{device}/On2/{level}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void On2(string device, string level);

        [OperationContract]
        [WebGet(UriTemplate = "/Level1Alert" , ResponseFormat=WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void Level1Alert();

        [OperationContract]
        [WebGet(UriTemplate = "/GetDevices2", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare)]
        Device[] GetDevices2();
    }
}
