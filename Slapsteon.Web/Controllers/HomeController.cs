using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slapsteon.Web.Models;
using Slapsteon.Web.Business;

namespace Slapsteon.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {


            return View();
        }

        public ActionResult List()
        {
            List<SlapsteonDevice> devices = (new SlapsteonFacade()).GetDevices();


            List<Device> deviceList = new List<Device>();

            foreach (SlapsteonDevice dev in devices)
            {
                int level;
                int.TryParse(dev.Status, out level);
                Device device = new Device();
                device.Address = dev.Address;
                device.Status = level;
                device.Name = dev.Name;
                device.LastOff = dev.LastOff;
                device.LastOn = dev.LastOn;
                device.IsFan = dev.IsFan;
                device.IsPLM = dev.IsPLM;
                device.IsDimmable = dev.IsDimmable;
                deviceList.Add(device);
            }

            DeviceList deviceListModel = new DeviceList(deviceList);

            return DeviceListView(deviceListModel);
        }

        private ActionResult DeviceListView(DeviceList deviceListModel)
        {
            return View("List", deviceListModel);
        }

        public ActionResult On(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.On(device.Name);

            return RedirectToAction("Index", "Home"); 
        }

        public ActionResult OnLevel(string deviceName, int? level)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.On(deviceName, level ?? 0);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult OnFan(string deviceName, int? level)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.On2(deviceName, level ?? 0);

            return RedirectToAction("Index", "Home"); 
        }

        public ActionResult Off(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.Off(device.Name);

            return RedirectToAction("Index", "Home");
        }
    }
}
