using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Slapsteon.Web.Models;
using Slapsteon.Web.Business;

namespace Slapsteon.Web.Controllers
{
    public class LightsController : Controller
    {
        //
        // GET: /Lights/

        public ActionResult Index()
        {
            List<SlapsteonDevice> devices = (new SlapsteonFacade()).GetDevices();

            Lights lightsModel = new Lights();
            lightsModel.AllLights = new List<Light>();

            foreach (SlapsteonDevice dev in devices)
            {
                int level;
                int.TryParse(dev.Status, out level);
                Light light = new Light(dev.Name, level);

                lightsModel.AllLights.Add(light);
            }

            return View(lightsModel);
        }

        //
        // GET: /Lights/Details/5

        public ActionResult Details(int id)
        {
            
            return View();
        }

        //
        // GET: /Lights/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Lights/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Lights/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Lights/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Lights/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Lights/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult On(Lights model)
        {
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult Off()
        {
            return new EmptyResult();
        }
    }
}
