using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slapsteon.Web.Models;
using Slapsteon.Web.Business;
using Insteon.Devices;

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


            List<DeviceOld> deviceList = new List<DeviceOld>();

            foreach (SlapsteonDevice dev in devices)
            {
                int level;
                int.TryParse(dev.Status, out level);
                DeviceOld device = new DeviceOld();
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

        public ActionResult List2()
        {
            List<Device> devices = (new SlapsteonFacade()).GetDevices2();

            return new EmptyResult();
        }

        private ActionResult DeviceListView(DeviceList deviceListModel)
        {
            return View("List", deviceListModel);
        }

        public ActionResult On(DeviceOld device)
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

        public ActionResult Off(DeviceOld device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.Off(device.Name);

            return RedirectToAction("Index", "Home");
        }
    }
}
