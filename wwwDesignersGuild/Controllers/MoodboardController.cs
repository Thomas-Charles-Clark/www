using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Twitter;
using wwwDesignersGuild.SignalR;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class MoodboardController : Controller
    {
        // GET: Moodboard
        [AccessControlAllowOrigin("*")]
        public FileContentResult Index(string fileName)
        {
            return new FileContentResult(System.IO.File.ReadAllBytes(Server.MapPath($"~/content/images/moodboards/{fileName}")), "image/jpeg");
        }

        // GET: TwitterCallback
        // Redirected to from Twitter Callback after Auth e.g. https://localhost:44302/moodboard/twitter-callback?oauth_token=TY2eEAAAAAABfWvwAAABglj-6Og&oauth_verifier=2YEs65pQ8FgfRZDifcAonGGfCQesn1ad
        public ActionResult TwitterCallback(string visitorId, string oauth_token, string oauth_verifier)
        {
            if (!String.IsNullOrEmpty(oauth_token) &&
                !String.IsNullOrEmpty(oauth_verifier))
            {
                GlobalHost.ConnectionManager.GetHubContext<MoodboardHub>()
                    .Clients.User(visitorId).receiveTwitterOAuth(new TwitterOAuthResponse(oauth_token, oauth_verifier));
            }

            return Content(null);
        }
    }
}