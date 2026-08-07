using Microsoft.Ajax.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.WebHost;
using System.Web.Routing;
using System.Web.SessionState;

namespace wwwDesignersGuild
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

			RouteTable.Routes.MapHttpRoute(
				name: "CacheBusterApi",
				routeTemplate: "api/cache/purge"
			).RouteHandler = new SessionStateReadOnlyRouteHandler();

			config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}"
            );
		}
	}

	// Custom handler that forces the pipeline into ReadOnly mode
	public class SessionStateReadOnlyRouteHandler : HttpControllerRouteHandler
	{
		protected override IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new SessionStateReadOnlyHandler(requestContext.RouteData);
		}
	}

	public class SessionStateReadOnlyHandler : HttpControllerHandler, IReadOnlySessionState
	{
		public SessionStateReadOnlyHandler(RouteData routeData) : base(routeData) { }
	}
}
