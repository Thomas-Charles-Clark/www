using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace wwwDesignersGuild.Controllers
{

    public class LinkShareController : Controller
    {


        //  Save linkshare cookie for uk
        //[RequireHttps]
        public ActionResult IndexUk()
        {


            String siteID = getLinkShareSiteID();
            if (siteID == "")
            {
                return Redirect("~/");
            }


            String url = getLinkShareUrl();
            if (url == "")
            {
                return Redirect("~/");
            }


            writeCookie("_linkshare_uk", siteID);


            return Redirect(url);
        }


        //  Save linkshare cookie for usa
        //[RequireHttps]
        public ActionResult IndexUsa()
        {
            

            String siteID = getLinkShareSiteID();
            if (siteID == "")
            {
                return Redirect("~/");
            }


            String url = getLinkShareUrl();
            if (url == "")
            {
                return Redirect("~/");
            }


            writeCookie("_linkshare_us", siteID);


            return Redirect(url);
        }


        #region Helper Routines


        private string getLinkShareSiteID()
        {

            if (Request.QueryString["siteID"] == null)
            {
                return "";
            }

            String siteId = Request.QueryString["siteID"];
            siteId = siteId.Trim();

            return siteId;
            

        }


        private string getLinkShareUrl()
        {

            if (Request.QueryString["url"] == null)
            {
                return "";
            }

            String url = Request.QueryString["url"];
            url = url.Trim();

            return url;


        }


        private void writeCookie(String name, String siteId)
        {
            HttpCookie cookie = new HttpCookie(name); ;
            cookie.Value = siteId;
            cookie.Expires = DateTime.Now.AddYears(2);
            cookie.SameSite = SameSiteMode.Lax;
            Response.Cookies.Add(cookie);

        }


        #endregion


    }
}