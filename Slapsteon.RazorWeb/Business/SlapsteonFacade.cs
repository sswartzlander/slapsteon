using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using Insteon.Devices;
using System.Runtime.Serialization.Json;
using Slapsteon.RazorWeb.Models;
using Newtonsoft.Json;

namespace Slapsteon.Web.Business
{
    public class SlapsteonFacade
    {

        public Device[] GetDevices()
        {
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
            }
            catch (Exception ex)
            {

            }
            return devArray;
        }

        public void On(string deviceName)
        {

            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/OnW/100/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void On(string deviceName, int level)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/OnW/{1}/{2}", deviceName, level, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void On2(string deviceName, int level)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/On2W/{1}/{2}", deviceName, level, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void Off(string deviceName)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/OffW/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void SetModeCool(string deviceName)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/SetModeCool/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void SetModeHeat(string deviceName)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/SetModeHeat/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void SetPointUp(string deviceName)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/SetPointUp/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public void SetPointDown(string deviceName)
        {
            string ip = GetClientIP();
            WebRequest request = WebRequest.Create(string.Format("http://localhost/InsteonWebService/{0}/SetPointDown/{1}", deviceName, ip)) as WebRequest;
            request.Method = "GET";
            request.ContentType = "application/json";
            request.ContentLength = 0;

            WebResponse response = request.GetResponse();

        }

        public SlapsteonEventLogEntry[] GetLog()
        {
            SlapsteonEventLogEntry[] logArray = null;
            try
            {
                WebRequest request = WebRequest.Create("http://localhost/InsteonWebService/Log") as WebRequest;
                request.Method = "GET";
                request.ContentType = "application/json";
                request.ContentLength = 0;

                WebResponse response = request.GetResponse();
                string responseString = null;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    responseString = reader.ReadToEnd();

                logArray = JsonConvert.DeserializeObject<SlapsteonEventLogEntry[]>(responseString);
            }
            catch (Exception ex)
            {

            }
            return logArray;
        }

        private string GetClientIP()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
    }
}