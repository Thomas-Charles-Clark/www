using Microsoft.AspNet.SignalR;

using NLog;

using StackExchange.Profiling;
using StackExchange.Profiling.Mvc;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;
using System.Web.WebPages;

using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.SignalR;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild
{
    public class MvcApplication : System.Web.HttpApplication
    {
		// Thread-safe dictionary to store and access your singleton caches
		public static Dictionary<string, MemoryCache> CacheSingletons { get; private set; } = new Dictionary<string, MemoryCache>();

		internal static readonly List<string> cacheNames = new List<string>()
		{
			"Attributes",
			"Articles",
			"ArticleTypes",
			"ArticleSubTypes",
			"Banners",
			"BraintreeErrorResponses",
			"CareersPage",
			"Collections",
			"CollectionProductPageGrids",
			"ContactDetails",
			"Countries",
			"CountryDropDownCountries",
			"DotDigitalTrackingBehaviours",
			"EmailCapturePopupForms",
            "FilterIndexAttributeCombinations",
			"LandingPages",
			"LifestyleGalleryPopups",
			"Hotspots",
			"MobileDetections",
			"Menus",
            "OrderForms",
			"ProductPages",
			"ProductPageGrids",
			"ProductTypeOrderRestrictions",
			"Resources",
            "Redirects",
			"Scrapbooks",
            "Searches",
			"SearchProductTypes",
			"SiteSections",
			"Snippets",
			"StockStatuses",
			"TradeAccountRequestForms",
			"UnsubscribePages"
		};

		public static readonly Dictionary<string, string[]> CacheKeySchemas = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
	        { "Attributes", new[] { "LanguageID" } },
	        { "Articles", new[] { "ArticleID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "ArticleTypes", new[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "ArticleSubTypes", new[] { "ArticleTypeID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Banners", new[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch", "PlatformSwitch" } },
	        { "BraintreeErrorResponses", new[] { "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "CareersPage", new[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Collections", new[] { "CollectionID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" } },
	        { "CollectionProductPageGrids", new[] { "CollectionID", "LanguageID", "CountryID", "TradeSwitch", "TradeAccountID", "CountryMatched", "Date.Year", "Date.Month", "Date.Day" } },
	        { "ContactDetails", new[] { "TradeAccountID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Countries", Array.Empty<string>() },
	        { "CountryDropDownCountries", Array.Empty<string>() },
	        { "DotDigitalTrackingBehaviours", new[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "EmailCapturePopupForms", new[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "FilterIndexAttributeCombinations", Array.Empty<string>() },
	        { "LandingPages", new[] { "PageID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" } },
	        { "LifestyleGalleryPopups", new[] { "ProductPageID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Hotspots", new[] { "ContentID", "ContentType", "TradeAccountID", "DisplayTradePrice", "CountryID", "LanguageID", "Date", "ImageIDs", "CountryMatch" } },
	        { "MobileDetections", new[] { "UserAgent" } },
	        { "Menus", new[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" } },
	        { "OrderForms", Array.Empty<string>() },
	        { "ProductPages", new[] { "ProductPageID", "LanguageID", "CountryID", "TradeSwitch", "PlatformSwitch", "Date.Year", "Date.Month", "Date.Day" } },
	        { "ProductPageGrids", new[] { "PageSectionID", "LanguageID", "CountryID", "TradeSwitch", "TradeAccountID", "CountryMatch", "Date.Year", "Date.Month", "Date.Day" } },
	        { "ProductTypeOrderRestrictions", new[] { "TradeAccountID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Resources", new[] { "LanguageID", "CountryID" } },
	        { "Redirects", new[] { "Date.Year", "Date.Month", "Date.Day" } },
	        { "Scrapbooks", new[] { "VisitorID", "MemberID", "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Searches", Array.Empty<string>() },
	        { "SearchProductTypes", new[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "SiteSections", new[] { "PageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "Snippets", new[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "StockStatuses", new[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "TradeAccountRequestForms", new[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" } },
	        { "UnsubscribePages", Array.Empty<string>() }
        };

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
			// 2. Instantiate and store each MemoryCache exactly once
			foreach (string name in cacheNames)
			{
				CacheSingletons[name] = new MemoryCache(name);
			}

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

			System.Threading.ThreadPool.SetMinThreads(1000, 1000);
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
			
            if (HttpContext.Current.Request.Headers["UserAgent"] == "DesignersGuild_Cache_Bust" ||
				HttpContext.Current.Request.Headers["UserAgent"] == "DesignersGuild_Cache_Fill" ||
				HttpContext.Current.Request.Headers["UserAgent"] == "DG_K6_FloodTest_UserAgentString/1.0")
			{
				var path = HttpContext.Current.Request.Url.AbsolutePath;

				// Look for "cache" anywhere in the requested URL path, ignoring case
				if (path.IndexOf("/api/cache", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					// Tell the async module to completely ignore this request
					HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Disabled);

					// Physically remove the ASP.NET session cookie from the incoming headers
					if (HttpContext.Current.Request.Cookies["ASP.NET_SessionId"] != null)
					{
						HttpContext.Current.Request.Cookies.Remove("ASP.NET_SessionId");
					}
				}

				if (Request.UserAgent == "DesignersGuild_Cache_Fill")
				{
					HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.ReadOnly);
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
			HttpContext.Current.Session.Add("memberTradeChecksPassed", false); // definitely used!
			HttpContext.Current.Session.Add("serverSearchResult", null); // no longer used: why would we need serverSearchResult in session...?
			HttpContext.Current.Session.Add("backInStockReminder", null); // definitely used: if you attempt to create a BISR without being logged in you get prompted to log in. Then once logged in, the BISR should be automatically created and you informed.
			HttpContext.Current.Session.Add("countryCorrectorDismissed", false); // definitely used: country nag
			HttpContext.Current.Session.Add("tradeLoginPromptDismissed", false); // definitely used: trade login nag
			HttpContext.Current.Session.Add("tradeOrderingAllowed", false); // definitely used: whether or not trade ordering is allowed
		}
    }
}