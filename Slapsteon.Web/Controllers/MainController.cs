using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insteon.Devices;
using Slapsteon.Web.Business;

namespace Slapsteon.Web.Controllers
{
    public class MainController : Controller
    {
        //
        // GET: /Main/

        public ActionResult Index()
        {
            var model = (new SlapsteonFacade()).GetDevices2();
            return View();
        }

    }
}
