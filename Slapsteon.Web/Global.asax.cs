﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Slapsteon.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
            routes.MapRoute(
                "OnLevel",
                "{controller}/OnLevel/{deviceName}/{level}",
                new { controller = "Home", action = "OnLevel" }
                );
            routes.MapRoute(
                "OnFan",
                "{controller}/OnFan/{deviceName}/{level}",
                new { controller = "Home", action = "OnFan" }
                );
        }

        protected void Application_Start()
        {
            WebPageHttpHandler.RegisterExtension("cshtml");
            RegisterRoutes(RouteTable.Routes);
        }
    }
}