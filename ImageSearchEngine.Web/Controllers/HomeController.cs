using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ImageSearchEngine.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult Text()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult View3D()
        {
            return View("View3D", "_Layout3D");
        }
    }
}
