using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using System.Web.SessionState;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Database;
using System.Web.WebPages;
using StackExchange.Profiling;
using StackExchange.Profiling.Mvc;
using wwwDesignersGuild.Util;
using Microsoft.AspNet.SignalR;
using wwwDesignersGuild.SignalR;

namespace wwwDesignersGuild
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region WebApi SessionState Allow


        protected void Application_PostAuthorizeRequest()
        {
            if (IsWebApiRequest())
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }


        private static bool IsWebApiRequest()
        {
            return HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath.StartsWith("~/api");
        }


        #endregion


        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            GlobalHost.DependencyResolver.Register(typeof(IUserIdProvider), () => new UserIdProvider());

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            DisplayModeProvider.Instance.Modes.Insert(0, new DefaultDisplayMode()
            {
                ContextCondition = context => dgFramework.IsMobileDevice() == false
            });

            DisplayModeProvider.Instance.Modes.Insert(1, new DefaultDisplayMode("mobile")
            {
                ContextCondition = context => dgFramework.IsMobileDevice()
            });

            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["enableProfiler"]) && HtmlHelperExtra.IsNonProductionVersion)
            {
                MiniProfiler.Configure(new MiniProfilerOptions
                {
                    RouteBasePath = "~/profiler",
                    PopupRenderPosition = RenderPosition.Right,  // defaults to left
                    PopupMaxTracesToShow = 10,                   // defaults to 15
                    ColorScheme = ColorScheme.Auto,              // defaults to light
                    ResultsAuthorize = request => request.IsLocal,
                    ResultsListAuthorize = request =>
                    {
                        // you may implement this if you need to restrict visibility of profiling lists on a per request basis
                        return true; // all requests are legit in this example
                    },
                    StackMaxLength = 512,
                    TrackConnectionOpenClose = true
                }
                // Optional settings to control the stack trace output in the details pane, examples:
                .ExcludeType("SessionFactory")  // Ignore any class with the name of SessionFactory)
                .ExcludeAssembly("NHibernate")  // Ignore any assembly named NHibernate
                .ExcludeMethod("Flush")         // Ignore any method with the name of Flush
                .AddViewProfiling()              // Add MVC view profiling (you want this)
                                                 // If using EntityFrameworkCore, here's where it'd go.
                                                 // .AddEntityFramework()        // Extension method in the MiniProfiler.EntityFrameworkCore package
                );
            }
        }

        protected void Application_BeginRequest()
        {
            //Note: This is now being controlled in the FilterConfig (per controlled)
            //HttpContext.Current.Response.Cache.SetAllowResponseInBrowserHistory(false);

            ////These two lines are causing IE to fail on downloading the embedded fonts correctly first time in.
            //HttpContext.Current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //HttpContext.Current.Response.Cache.SetNoStore();            

            //Response.Cache.SetExpires(DateTime.Now);
            //Response.Cache.SetValidUntilExpires(true);

            if (Convert.ToBoolean(ConfigurationManager.AppSettings["enableProfiler"]) && HtmlHelperExtra.IsNonProductionVersion)
            {
                // You can decide whether to profile here, or it can be done in ActionFilters, etc.
                // We're doing it here so profiling happens ASAP to account for as much time as possible.
                if (Request.IsLocal) // Example of conditional profiling, you could just call MiniProfiler.StartNew();
                {
                    MiniProfiler.StartNew();
                }
            }
        }

        protected void Application_EndRequest()
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["enableProfiler"]) && HtmlHelperExtra.IsNonProductionVersion)
            {
                MiniProfiler.Current?.Stop(); // Be sure to stop the profiler!
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();

            if (exc is PageNotFoundException nfe)
            {
                HttpContext.Current.ClearError();
                HttpContext.Current.Response.Redirect($"~/{nfe.SiteCulture}/l0");
                return;
            }

            if (HtmlHelperExtra.IsProductionVersion)
            {
                String urlOfPage = "unknown";
                if (Request == null) { }
                else
                {
                    if (Request.Url == null) { }
                    else
                    {
                        urlOfPage = Request.Url.ToString();
                    }
                }

                string stackTrace = "";
                string innerExceptionType = "";
                string innerExceptionMessage = "";
                string innerExceptionSource = "";
                string innerExceptionStackTrace = "";


                //  Get exception details
                string exceptionType = exc.GetType().ToString();
                string exceptionMessage = exc.Message;
                string exceptionSource = exc.Source;

                if (exc.StackTrace != null)
                {
                    stackTrace = exc.StackTrace;
                }

                //  Get inner exception details
                if (exc.InnerException == null) { }
                else
                {
                    innerExceptionType = exc.InnerException.GetType().ToString();
                    innerExceptionMessage = exc.InnerException.Message;
                    innerExceptionSource = exc.InnerException.Source;

                    if (exc.InnerException.StackTrace != null)
                    {
                        innerExceptionStackTrace = exc.InnerException.StackTrace;
                    }
                }

                Server.ClearError();


                //  Get visitor id
                Int64 visitorDbId = -1;
                if (HttpContext.Current.Session == null) { }
                else
                {
                    if (HttpContext.Current.Session["visitorIdFromDatabase"] == null) { }
                    else
                    {
                        visitorDbId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
                    }

                }


                //  Determine if the error message should be shown
                Boolean includeErrorInLog = true;
                if (System.Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
                else
                {
                    includeErrorInLog = false;
                }
                if (exceptionType == "System.Web.HttpRequestValidationException")
                {
                    includeErrorInLog = false;
                }
                if (visitorDbId == -1)
                {
                    includeErrorInLog = false;
                }


                try
                {

                    //  Write exception to the database
                    if (includeErrorInLog == true)
                    {
                        Logger.Error(exc, "Error occurred: ");

                        errorDB objError = new errorDB();
                        objError.saveToErrorLog(visitorDbId, exceptionType, exceptionMessage, exceptionSource, stackTrace,
                                                    innerExceptionType, innerExceptionMessage, innerExceptionSource, innerExceptionStackTrace,
                                                    urlOfPage, dgFramework.getUserAgent(), dgFramework.getIPAddress());
                    }

                }
                catch (Exception)
                {
                    //  Ignore error message
                }

                Server.TransferRequest("/major-error", false);
            }
        }


        protected void Session_Start()
        {
            HttpContext.Current.Session.Add("loginStatus", dgLoginStatus.notLoggedIn.ToString()); // definitely used
            HttpContext.Current.Session.Add("memberName", null); // definitely used
            HttpContext.Current.Session.Add("memberEmail", null); // definitely used
            HttpContext.Current.Session.Add("visitorIdFromDatabase", null); // definitely used
            HttpContext.Current.Session.Add("keepSessionAlive", DateTime.Now.ToString()); // definitely used
            HttpContext.Current.Session.Add("checkoutId", null); // definitely used, maintains checkoutId
            HttpContext.Current.Session.Add("checkoutExpiry", null); // definitely used, expires checkout
            HttpContext.Current.Session.Add("checkoutDisplayGiftTab", false); // not used?
            HttpContext.Current.Session.Add("checkoutDisplayGiftVoucherTab", false); // not used?
            HttpContext.Current.Session.Add("serverSearchResult", null); // no longer used: why would we need serverSearchResult in session...?
            HttpContext.Current.Session.Add("backInStockReminder", null); // definitely used: if you attempt to create a BISR without being logged in you get prompted to log in. Then once logged in, the BISR should be automatically created and you informed.
            HttpContext.Current.Session.Add("countryCorrectorDismissed", false); // definitely used: if you attempt to create a BISR without being logged in you get prompted to log in. Then once logged in, the BISR should be automatically created and you informed.
        }
    }
}