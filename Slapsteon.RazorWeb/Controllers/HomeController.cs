using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slapsteon.Web.Business;
using Insteon.Devices;
using Slapsteon.RazorWeb.Models;

namespace Slapsteon.RazorWeb.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            SlapsteonDisplayModel model = new SlapsteonDisplayModel();

            Device[] allDevices = (new SlapsteonFacade()).GetDevices();
            model.Devices_Floor1 = allDevices.Where(d => d.Floor == "1").ToArray();
            model.Devices_Floor2 = allDevices.Where(d => d.Floor == "2").ToArray();
            model.Devices_Exterior = allDevices.Where(d => d.Floor == "E").ToArray();
            model.Devices_Basement = allDevices.Where(d => d.Floor == "B").ToArray();
            model.Events = (new SlapsteonFacade()).GetLog();
            return View(model);
        }

        private ActionResult GetLog()
        {
            SlapsteonEventLogEntry[] eventLog = (new SlapsteonFacade()).GetLog();

            return PartialView(eventLog);
        }

        public ActionResult On(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.On(device.Name);
            device.Status = 100;
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
            device.Status = 0;

            return RedirectToAction("Index","Home");

        }

        public ActionResult SetCoolMode(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.SetModeCool(device.Name);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SetHeatMode(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.SetModeHeat(device.Name);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SetPointUp(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.SetPointUp(device.Name);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SetPointDown(Device device)
        {
            SlapsteonFacade facade = new SlapsteonFacade();
            facade.SetPointDown(device.Name);
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Refresh(Device device)
        {
            return PartialView(device);
        }
    }
}
