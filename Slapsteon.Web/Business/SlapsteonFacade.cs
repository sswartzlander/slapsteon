using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Slapsteon.Web.Models;
using Insteon.Devices;
using Newtonsoft.Json.Serialization;
using System.Runtime.Serialization.Json;

namespace Slapsteon.Web.Business
{
    public class SlapsteonFacade
    {
        public List<SlapsteonDevice> GetDevices()
        {
            List<SlapsteonDevice> devices = new List<SlapsteonDevice>();
            SlapsteonDevice[] slapsteonDevices = null;

            try
            {
                WebRequest request = WebRequest.Create("http://localhost/InsteonWebService/Devices") as WebRequest;
                request.Method = "GET";
                request.ContentType = "application/json";
                request.ContentLength = 0;

                WebResponse response = request.GetResponse();
                string responseString = null;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    responseString = reader.ReadToEnd();

                slapsteonDevices = JsonConvert.DeserializeObject<SlapsteonDevice[]>(responseString);
                devices = slapsteonDevices.ToList();
            }
            catch (Exception ex)
            {

            }
            return devices;
        }

        public List<Device> GetDevices2()
        {
            List<Device> devices = new List<Device>();
            Device[] devArray = null;
            try
            {
                WebRequest request = WebRequest.Create("http://localhost/InsteonWebService/GetDevices2") as WebRequest;
                request.Method = "GET";
                request.ContentType = "application/json";
                request.ContentLength = 0;

                WebResponse response = request.GetResponse();

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Device[]));
                devArray = (Device[])serializer.ReadObject(response.GetResponseStream());
                devices = devArray.ToList();
            }
            catch (Exception ex)
            {

            }
            return devices;
        }

        public void On(string deviceName)
        {
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/On/100",deviceName)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void On(string deviceName, int level)
        {
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/On/{1}", deviceName, level)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void On2(string deviceName, int level)
        {
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/On2/{1}", deviceName, level)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void Off(string deviceName)
        {
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/Off", deviceName)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }
    }

    //private class DeviceJsonSerializer<T>
    //{
    //    private IContractResolver _resolver;

    //    public DeviceJsonSerializer(IContractResolver resolver)
    //    {
    //        _resolver = resolver;
    //    }

    //    public List<R> FromJson<List<R>>(object json)
    //    {
    //        if (typeof(T).IsAssignableFrom(typeof(R)))
    //        {
    //            object res = JsonConvert.Des
    //        }
    //    }
    //}
}

/*
 WebRequest request = WebRequest.Create(
                string.Format("http://{0}/ConfigurationInterface/components", serverAddress)
                ) as WebRequest;
                request.Method = "GET";
                request.ContentType = "application/json";
                request.ContentLength = 0;

                WebResponse response = request.GetResponse();

                string responseString = null;
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    responseString = reader.ReadToEnd();

                result = JsonConvert.DeserializeObject<DeployedComponentDTO[]>(responseString);

                return result;

*/