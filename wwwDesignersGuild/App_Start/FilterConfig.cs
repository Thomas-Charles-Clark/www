using System;
using System.Web;
using System.Web.Mvc;

namespace wwwDesignersGuild
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new RequireHttpsAttribute());
            filters.Add(new ControllerCache());    //  Control the caching per controller
        }
    }

    class ControllerCache : ActionFilterAttribute
    {
        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.RouteData.Values["controller"].ToString() == "Image")
            {
                //  Allow images to be cached on the client
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
                filterContext.HttpContext.Response.Cache.SetMaxAge(new TimeSpan(365, 24, 0, 0));
                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.Now.Add(new TimeSpan(365, 24, 0, 0)));
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(true);
            }
            else
            {
                //  Stop all caching in the other controllers
                filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.NoCache);

                filterContext.HttpContext.Response.Cache.SetAllowResponseInBrowserHistory(false);
                filterContext.HttpContext.Response.Cache.SetNoStore();

                filterContext.HttpContext.Response.Cache.SetExpires(DateTime.Now);
                filterContext.HttpContext.Response.Cache.SetValidUntilExpires(true); 
            }
        }
    }
}