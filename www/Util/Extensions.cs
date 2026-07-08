using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.UI;

namespace wwwDesignersGuild.Util
{
    public static class MathExtensions
    {
        public static int Clamp(this int value, int min, int max)
        {
            return value <= min ? min : (value >= max) ? max : value;
        }
    }

    // With thanks: https://www.codemag.com/article/1312081/Rendering-ASP.NET-MVC-Razor-Views-to-String
    public static class ViewRenderer
    {
        internal static string RenderViewToString(
            ControllerContext context,
            string viewPath,
            object model = null,
            bool partial = false)
        {
            // first find the ViewEngine for this view
            ViewEngineResult viewEngineResult = null;

            if (partial)
                viewEngineResult = ViewEngines.Engines.FindPartialView(context, viewPath);
            else
                viewEngineResult = ViewEngines.Engines.FindView(context, viewPath, null);

            if (viewEngineResult == null)
                throw new FileNotFoundException("View cannot be found.");

            // get the view and attach the model to view data
            var view = viewEngineResult.View;
            context.Controller.ViewData.Model = model;

            string result = null;

            using (var sw = new StringWriter())
            {
                var ctx = new ViewContext(context, view, context.Controller.ViewData, context.Controller.TempData, sw);
                view.Render(ctx, sw);
                result = sw.ToString();
            }

            return result;
        }

        public static T CreateController<T>(RouteData routeData = null) where T : Controller, new()
        {
            // create a disconnected controller instance
            T controller = new T();

            // get context wrapper from HttpContext if available
            HttpContextBase wrapper;


			// 1. Create the base dummy context
			var request = new SimpleWorkerRequest("", "", new StringWriter());
			var fakeContext = new HttpContext(request);

			// 2. Build the fake session storage container
			var sessionItems = new SessionStateItemCollection();
			// Optional: Preload any data your view needs here
			// sessionItems["SomeKey"] = "SomeValue";

			var sessionContainer = new HttpSessionStateContainer(
				id: $"FakeSessionId-{DateTime.Now.Ticks}",
				sessionItems: sessionItems,
				staticObjects: new HttpStaticObjectsCollection(),
				timeout: 20,
				newSession: true,
				cookieMode: HttpCookieMode.UseCookies,
				mode: SessionStateMode.InProc,
				isReadonly: false
			);

			// 3. Use reflection to build the internal HttpSessionState wrapper
			var sessionStateType = typeof(HttpSessionState);
			var constructor = sessionStateType.GetConstructor(
				BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				new Type[] { typeof(IHttpSessionState) },
				null
			);

			if (constructor == null)
			{
				throw new InvalidOperationException("Could not find internal HttpSessionState constructor.");
			}

			var actualSessionState = (HttpSessionState)constructor.Invoke(new object[] { sessionContainer });

			// 4. CRITICAL: Force the HttpContext to cache the session by setting the internal field
			var sessionField = typeof(HttpContext).GetField("_session", BindingFlags.NonPublic | BindingFlags.Instance);
			if (sessionField != null)
			{
				sessionField.SetValue(fakeContext, actualSessionState);
			}

			if (System.Web.HttpContext.Current != null)
                wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
            else
            {
                wrapper = new HttpContextWrapper(fakeContext);
			}

            if (routeData == null)
                routeData = new RouteData();

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller", controller.GetType().Name.ToLower().Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }
    }
    
    public class GenericController : Controller
    {
    
    }

    // With thanks https://stackoverflow.com/questions/7802822/all-possible-combinations-of-a-list-of-values#answer-57058345
    // further explained by https://stackoverflow.com/questions/57948316/why-is-bitwise-operator-needed-in-this-powerset-generator#answer-57948404
    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> PowerSet<T>(IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            T[] data = source.ToArray();

            return Enumerable
                .Range(1, 1 << data.Length)
                .Select(index => {
                    return data
                        .Where((d, i) => {
                            return (index & (1 << i)) != 0;
                        })
                        .ToArray();
                });
        }
    }
}