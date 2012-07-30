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
        [WebGet(UriTemplate = "/GameroomDimmer/On", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void GameroomDimmerOn();

         [OperationContract]
        [WebGet(UriTemplate = "/GameroomDimmer/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        void GameroomDimmerOff();

         [OperationContract]
         [WebGet(UriTemplate = "/LivingRoomDimmer/On", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void LivingRoomDimmerOn();

         [OperationContract]
         [WebGet(UriTemplate = "/LivingRoomDimmer/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void LivingRoomDimmerOff();

         [OperationContract]
         [WebGet(UriTemplate = "/LivingRoomDimmer/RampOn", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void LivingRoomDimmerRampOn();

         [OperationContract]
         [WebGet(UriTemplate = "/LivingRoomDimmer/RampOff", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void LivingRoomDimmerRampOff();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerOff();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/On100", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerOn100();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/On40", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerOn40();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/On70", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerOn70();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/On30", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerOn30();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/RampOn", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerRampOn();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRDimmer/RampOff", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRDimmerRampOff();

         [OperationContract]
         [WebGet(UriTemplate = "/KitchenMulti/On", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void KitchenMultiOn();

         [OperationContract]
         [WebGet(UriTemplate = "/KitchenMulti/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void KitchenMultiOff();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRMulti/On", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRMultiOn();

         [OperationContract]
         [WebGet(UriTemplate = "/MBRMulti/Off", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         void MBRMultiOff();

         [OperationContract]
         [WebGet(UriTemplate = "/GetAddressTable/{name}", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
         string GetAddressTable(string name);
    }
}
