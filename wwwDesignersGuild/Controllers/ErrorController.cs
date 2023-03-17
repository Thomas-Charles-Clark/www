using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Controllers
{
    public class ErrorController : Controller
    {
        
        public ActionResult HandleError()
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Error/MajorError{0}.cshtml"));
        }

    }
}