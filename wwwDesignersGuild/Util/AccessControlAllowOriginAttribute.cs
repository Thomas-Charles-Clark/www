using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace wwwDesignersGuild.Util
{
    public class AccessControlAllowOriginAttribute : ActionFilterAttribute
    {
        public string Value { get; set; }

        public AccessControlAllowOriginAttribute(string value)
        {
            Value = value;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", Value);
            base.OnActionExecuting(filterContext);
        }
    }
}