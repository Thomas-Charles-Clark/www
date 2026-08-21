using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace wwwDesignersGuild.Controllers
{
	[SessionState(SessionStateBehavior.ReadOnly)]
	public class HeartbeatController : Controller
    {
		// POST: Heartbeat
		[HttpPost]
		public ActionResult Bradycardia()
        {
            return new HttpStatusCodeResult(System.Net.HttpStatusCode.NoContent);
        }
    }
}