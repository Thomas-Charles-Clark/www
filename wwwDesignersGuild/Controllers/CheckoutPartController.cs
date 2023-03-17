using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace wwwDesignersGuild.Controllers
{
    public class CheckoutPartController : Controller
    {
        // GET: CheckoutPart
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Address()
        {
            return View();
        }
    }
}