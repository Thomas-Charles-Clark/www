using NLog;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.ArticlePage;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.CollectionPage;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Redirects;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.PCAFind;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Controllers
{
    public class HomeController : Controller
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        public bool IsShop { get; set; }

        /// <summary>
        /// CatchAll route handler.
        /// </summary>
        /// <param name="urlParameter">Everything after the .com/</param>
        /// <returns></returns>
        /// 
        //[RequireHttps]
        public async Task<ActionResult> CatchAll(string urlParameter)
        {
            if (urlParameter == "fr/bienvenue-chez-designers-guild/l0")
            {
                return new RedirectResult("/fr/l0", true);
            }
            
            #region Here we go!

            #region Lordy, lordy lord... IIS is rewriting multiple///empty/segments into urls/without/empty/segments which is preventing me from redirecting!

            if (Request.ServerVariables["HTTP_URL"] != null)
            {
                urlParameter = Request.ServerVariables["HTTP_URL"];

                if (urlParameter.IndexOf("?") > 0)
                {
                    urlParameter = urlParameter.Substring(0, urlParameter.IndexOf("?"));
                }
            }

            urlParameter = urlParameter.TrimStart(Framework.ForwardSlash);

            #endregion

            Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includesSubDomains");
            
            if (Request.QueryString.AllKeys.Contains("partial") || Request.QueryString.AllKeys.Contains("lifestyle"))
            {
                ViewBag.Partial = true;
            }
            else
            {
                ViewBag.Partial = false;
            }

            if (Request.QueryString.AllKeys.Contains("lifestyle"))
            {
                IsShop = false;
            }
            else
            {
                IsShop = true;
            }

            var cacheTimeMinutes = Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]);

            var DefaultContentType = dgContentType.landingPage;

            var DefaultContentId = (int)dgFixedPage.home;

            var objVisitorFactory = new VisitorFactory();

            var objVisitorSelect = new VisitorSelect();

            var objRedirectRepository = new RedirectRepository();

            var forceTradeAccountCheck = false;

            #endregion


            #region Initial Tidy of urlParameter

            // Handle null
            if (urlParameter == null)
            {
                urlParameter = "";
            }

            // Remove any trailing '/'
            urlParameter = urlParameter.TrimEnd('/');

            #endregion


            #region Determine ContentDate

            //  Determine the date to use when obtaining data from the cms
            var useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);

            // Only if we're not in Production mode can we travel through time...
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null) { }
                else
                {
                    string overrideDate = Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            // If we've failed to parse a DateTime from the given parameter, use today's date.
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            #endregion


            #region Get Country & Language data pub.Country_Language_Select (results from in-memory cache or db)

            //  Get the country information
            var objCountry = new cmsCountry();
            var objCountryData = await objCountry.countryLanguageSelect(useDate);

            #endregion
            

            #region Get VisitorID, set a few variables in Session

            //  Get the session identifier
            if (Session["visitorIdFromDatabase"] == null)
            {
                // Looks for VisitorID using GUID in cookie

                string cookieGuid = dgFramework.getIdentificationFromCookie();
                
                objVisitorSelect = await dgFramework.identifySession(objVisitorSelect, objVisitorFactory, cookieGuid, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);
                
                //Rebuild session variables from objVisitorSelect
                Session["memberName"] = objVisitorSelect.FullName;
                Session["memberEmail"] = objVisitorSelect.Email;
                Session["visitorCulture"] = objVisitorSelect.Culture;
                Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
                Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
                Session["memberTradePriceListID"] = objVisitorSelect.tradePriceListId;
                Session["memberDisplayTradePrice"] = objVisitorSelect.memberDisplayTradePrice;
                Session["memberTradeDataSourceJson"] = objVisitorSelect.memberTradeDataSourceJson;
                Session["visitorCreatedDT"] = objVisitorSelect.VisitorCreatedDT;
                
                // And identifySession may have returned a visitorDbId of -1
                if (objVisitorSelect.visitorDbId == -1)
                {
                    Session["loginStatus"] = dgLoginStatus.notLoggedIn.ToString();
                }
                else
                {
                    // however, if identifySession didn't return -1 then store the visitorDbId in Session.
                    Session["visitorIdFromDatabase"] = objVisitorSelect.visitorDbId.ToString();
                    Session["memberIdFromDatabase"] = objVisitorSelect.memberId;
                    
                    if (objVisitorSelect.FullName == null && objVisitorSelect.Email == null) { }
                    else
                    {
                        Session["loginStatus"] = dgLoginStatus.previousLogin.ToString();
                    }
                }
            }
            else
            {
                objVisitorSelect.visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
                objVisitorSelect.memberTradeAccountNumber = (string)Session["memberTradeAccountNumber"];
                objVisitorSelect.memberTradeAccountID = (int?)Session["memberTradeAccountID"];
                objVisitorSelect.memberId = (Int32?)Session["memberIdFromDatabase"];
                objVisitorSelect.Email = (string)Session["memberEmail"];
                objVisitorSelect.memberDisplayTradePrice = (Boolean)Session["memberDisplayTradePrice"];
                objVisitorSelect.memberTradeDataSourceJson = (string)Session["memberTradeDataSourceJson"];
                objVisitorSelect.VisitorCreatedDT = (DateTime)Session["visitorCreatedDT"];
                objVisitorSelect.tradePriceListId = (short?)Session["memberTradePriceListID"];

                if (Session["visitorCulture"] == null) { }
                else
                {
                    objVisitorSelect.Culture = Session["visitorCulture"].ToString();
                }
            }

            if (Session["loginStatus"] == null)
            {
                Session["loginStatus"] = dgLoginStatus.notLoggedIn.ToString();
            }

            Session["CF-IPCountry"] = Request.Headers["CF-IPCountry"];

            #endregion


            #region Process urlParameter

            // Need to check if this is the Twitter API returning someone to our callback URL
            if (urlParameter == "moodboard/twitter-callback")
            {
                return RedirectToAction("TwitterCallback", "Moodboard", new { 
                    visitorId = Request.Cookies["_dgVisitIdent"].Value,
                    oauth_token = Request.QueryString["oauth_token"],
                    oauth_verifier = Request.QueryString["oauth_verifier"]
                });
            }


            // Parsing of the URLParameter, determining Country_Language and storing in urlData.countryItem - Defaults to GB (Eng)
            var urlData = await buildUrlBreakdown(urlParameter, objCountryData, objVisitorSelect, useDate);

            //if (urlData.siteCountryNotFoundInUrl && !urlData.contentTypeAndIdNotFound)
            //{
            //    return new RedirectResult(dgFramework.buildUrl(urlData.countryDetails, urlData.SemanticString, urlData.ContentType, urlData.ContentId));
            //}

            var urlParameterWithoutSuffix = urlParameter;

            if (!String.IsNullOrEmpty(urlParameterWithoutSuffix))
            {
                var split = urlParameterWithoutSuffix.Split(new char[] { '/' });

                urlParameterWithoutSuffix = String.Join("/", split.Skip(0).Take(split.Count() - 1));
            }

            #endregion


            #region Create or Update Visitor

            // If we have a VisitorID to use, this saves the "Culture" column on [mbr].[Visitor]
            // If not, this creates a new Visitor storing UserAgent, IP and Culture, saves the returned GUID to cookie and gives us the VisitorID
            if (objVisitorSelect.visitorDbId == -1)
            {
                identificationDefinition identDef = await dgFramework.generateNewVisitorId(objVisitorFactory, dgFramework.getUserAgent(), dgFramework.getIPAddress(), urlData.countryDetails.URL, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);
                
                objVisitorSelect.visitorDbId = identDef.visitorDbId;

                dgFramework.saveIdentificationToCookie(identDef.userGuid);

                Session["memberTradePriceListID"] = objVisitorSelect.tradePriceListId;
                Session["memberIdFromDatabase"] = objVisitorSelect.memberId;
                Session["visitorIdFromDatabase"] = objVisitorSelect.visitorDbId.ToString();
                Session["visitorCulture"] = urlData.countryDetails.URL;
                Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
            }
            else
            {
                if (objVisitorSelect.Culture == null)
                {
                    // No culture, which is weird and wrong. This should never be hit.
                }
                else
                {
                    // The identified visitor has a culture. If they were heading for the main landing page, redirect them to their culture's home page.
                    if (urlData.ContentId == -1) 
                    {
                        urlData.countryDetails = objCountryData.countryItems.Single(ci => ci.URL == objVisitorSelect.Culture);
                        urlData.ContentId = 0;

                        var url = dgFramework.buildUrl(
                                urlData.countryDetails,
                                null,
                                dgContentType.landingPage,
                                urlData.ContentId,
                                !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null);
                        
                        return new RedirectResult(url, false); // NOT a permanent redirect.
                    }
                    else
                    {
                        if (objVisitorSelect.Culture == urlData.countryDetails.URL)
                        {
                            // No need to update mbr.Visitor
                        }
                        else
                        {
                            forceTradeAccountCheck = true;

                            Boolean tmpSuccess = await updateVisitor(objVisitorSelect.visitorDbId, dgFramework.getIPAddress(), urlData.countryDetails.URL, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

                            Session["visitorCulture"] = urlData.countryDetails.URL;
                        }
                    }
                }
            }
            #endregion
            

            #region Promotion Code query string parsing

            var queryStringDictLowerCaseKeys = Request.QueryString.AllKeys.ToDictionary(k => k ?? "".ToLowerInvariant(), k => Request.QueryString[k]);

            if (queryStringDictLowerCaseKeys.ContainsKey("promocode"))
            {
                var promoCodeDb = new promotionDb();

                var code = queryStringDictLowerCaseKeys["promocode"];

                if (code.Length > 20)
                    code = code.Substring(0, 20);

                promoCodeDb.callAddPromotionCode(Convert.ToInt64(Session["visitorIdFromDatabase"]), objVisitorSelect.memberId, null, urlData.countryDetails.CountryID, code);
            }

            #endregion


            #region Test for Redirect

            var objRedirects = await objRedirectRepository.loadRedirectsFromCms(useDate);

            var objRedirect = objRedirects.Redirects.SingleOrDefault(r => r.OldURL == $"/{urlParameter}");

            if (objRedirect != null)
            {
                // Redirect!
                return new RedirectResult(objRedirect.RedirectURL, objRedirect.Permanent);
            }

            #endregion


            #region Potential Redirect:  urlData.contentTypeAndIdNotFound

            // 302
            //
            // Have we parsed a valid ContentType character and a valid number from your URLParameter?
            // if not, boom you're being redirected to the default (which was set in the Parse function)
            if (urlData.contentTypeAndIdNotFound == true)
            {
                var url = dgFramework.buildUrl(
                                urlData.countryDetails,
                                "",
                                DefaultContentType,
                                DefaultContentId,
                                !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null);
                
                // Don't index this URL: it was invalid!
                Response.Headers.Add("X-Robots-Tag", "noindex");

                Response.Headers.Add("cache-control", "no-cache");

                return new RedirectResult(url, true);
            }

            #endregion


            #region Correct Site Checking
            
            // Get the CountryCode header from CloudFlare.
            var cfIPCountryCode = Request.Headers["CF-IPCountry"] ?? Request.Headers["CF-IPCountry-Override"];

            // If we're not in production, permit countryOverride query string parameter.
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                if (Request.QueryString["countryOverride"] == null)
                {
                    if (String.IsNullOrEmpty(cfIPCountryCode))
                    {
                        // This should only be hit when developing locally.
                        // You're not in production, not used a countryOverride, not got a CountryCode from CloudFlare.
                        cfIPCountryCode = urlData.countryDetails.CountryCode;
                    }
                }
                else
                {
                    cfIPCountryCode = Request.QueryString["countryOverride"];
                    //if (dgFramework.doesCountryExistFromText(Request.QueryString["countryOverride"], objCountryData))
                    //{
                    //    var cd = dgFramework.getCountryFromText(Request.QueryString["countryOverride"], objCountryData);
                    //    cfIPCountryCode = cd.CountryCode;
                    //}
                }
            }
            #endregion


            #region Visitor Identification & Session Establishment ... how redundant is this?

            if (urlData.countryDetails.AllowTradeOrders)
            {
                Session["tradeOrderingAllowed"] = true;
            }
            else
            {
                Session["tradeOrderingAllowed"] = false;
            }

            int? memberId = null;

            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)Session["memberIdFromDatabase"];
            }

            if (!dgFramework.isPagePartOfCheckOutProcess(urlData.ContentType, urlData.ContentId))
            {
                Session["checkoutId"] = null;
            }


            if (Session["tradeOrderingAllowed"] == null)
            {
                throw new ArgumentNullException("Session[\"tradeOrderingAllowed\"] is NULL");
            }

            if (Session["memberTradeChecksPassed"] == null || forceTradeAccountCheck)
            {
                await tradeAccountCheck(objCountryData, urlData, objVisitorSelect);
            }

            if (forceTradeAccountCheck)
            {
                string cookieGuid = dgFramework.getIdentificationFromCookie();

                objVisitorSelect = await dgFramework.identifySession(objVisitorSelect, objVisitorFactory, cookieGuid, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

                //Rebuild session variables from objVisitorSelect
                Session["memberName"] = objVisitorSelect.FullName;
                Session["memberEmail"] = objVisitorSelect.Email;
                Session["visitorCulture"] = objVisitorSelect.Culture;
                Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
                Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
                Session["memberTradePriceListID"] = objVisitorSelect.tradePriceListId;
                Session["memberDisplayTradePrice"] = objVisitorSelect.memberDisplayTradePrice;
                Session["memberTradeDataSourceJson"] = objVisitorSelect.memberTradeDataSourceJson;
                Session["visitorCreatedDT"] = objVisitorSelect.VisitorCreatedDT;

                await tradeAccountCheck(objCountryData, urlData, objVisitorSelect);
            }

            if (Convert.ToBoolean(Session["memberTradeChecksPassed"]) == false)
            {
                Session["memberTradeAccountID"] = null;
                Session["memberTradeAccountNumber"] = null;
                Session["memberTradePriceListID"] = null;
                Session["memberDisplayTradePrice"] = false;
                Session["memberTradeDataSourceJson"] = null;
            }

            if (Session["memberDisplayTradePrice"] == null)
            {
                throw new ArgumentNullException("Session[\"memberDisplayTradePrice\"] is NULL");
            }

            // Act on the countryCode.
            if (!String.IsNullOrEmpty(cfIPCountryCode))
            {
                var checkResult = objVisitorFactory.checkCountry(urlData?.countryDetails?.CountryID ?? 81, urlData?.countryDetails?.LanguageID ?? 1,
                    Session["memberTradeAccountID"] != null, cfIPCountryCode);

                if (checkResult.canSeeRRPs)
                {
                    Session["canViewRRPs"] = true;
                }
                else
                {
                    Session["canViewRRPs"] = false;
                }

                if (checkResult.countryItem.CountryID != urlData.countryDetails.CountryID)
                {
                    // You're in the wrong site.
                    // We want to suggest a better site culture for you.

                    Session["countryCheckMatched"] = false;
                    Session["countryCheckCultureSuggestion"] = checkResult.countryItem.URL;

                    urlData.CountryAwareness = new countryAwarenessCheckReturn()
                    {
                        Match = false,
                        Culture = checkResult.countryItem.URL,
                        objCountryItem = checkResult.countryItem
                    };
                }
                else
                {
                    // You're in the right site.
                    // We don't need to give you the country awareness prompt
                    Session["countryCheckMatched"] = true;
                    Session["countryCheckCultureSuggestion"] = null;

                    urlData.CountryAwareness = new countryAwarenessCheckReturn()
                    {
                        Match = true,
                        Culture = null,
                        objCountryItem = new countryItem()
                        {
                            URL = null
                        }
                    };
                }
            }
            else
            {
                // This should *never* be hit
                // In production, CloudFlare should always be able to provide us with a CountryCode header (hopefully!).
                Session["canViewRRPs"] = false;
                Session["countryCheckMatched"] = false;
                Session["countryCheckCultureSuggestion"] = null;
                urlData.CountryAwareness = new countryAwarenessCheckReturn()
                {
                    Match = false,
                    Culture = null,
                    objCountryItem = new countryItem()
                    {
                        URL = null
                    }
                };
            }

            #endregion


            #region Back in Stock Reminder

            if (Session["backInStockReminder"] != null)
            {
                if (
                    (
                        (urlData.ContentType != dgContentType.landingPage)
                        ||
                        (
                            urlData.ContentType == dgContentType.landingPage &&
                            urlData.ContentId != (int)dgFixedPage.memberLogin
                        )
                    )
                    &&
                    (string)Session["loginStatus"] != dgLoginStatus.currentLogin.ToString())
                {
                    Session["backInStockReminder"] = null;
                }
            }

            #endregion


            #region Build LandingPage

            //  Build the output
            if (urlData.ContentType == dgContentType.landingPage)
            {
                if (Enum.IsDefined(typeof(dgFixedPage), urlData.ContentId))
                {
                    var fixedPage = (dgFixedPage)Enum.Parse(typeof(dgFixedPage), $"{urlData.ContentId}");

                    var loginAttribute = fixedPage.GetAttributeOfType<LoginRequiredAttribute>();

                    if (loginAttribute.LoginRequired &&
                        (string)Session["loginStatus"] != dgLoginStatus.currentLogin.ToString())
                    {
                        var redirectUrl = dgFramework.buildUrl(
                            dgContentType.landingPage, 
                            (int)dgFixedPage.memberLogin, 
                            urlData.countryDetails, 
                            urlData.ContentId, 
                            dgContentType.landingPage,
                            null,
                            null,
                            Framework.ProductionVersion ? new List<string>() { "t" } : new List<string>() { "t", "dateOverride", "countryOverride" }
                        );

                        // Don't index this URL: it is login protected
                        Response.Headers.Add("X-Robots-Tag", "noindex");

                        return new RedirectResult(redirectUrl, false);
                    }
                    else
                    {
                        if (loginAttribute.LoginRequired && 
                            loginAttribute.TradeRequired)
                        {
                            if (
                                  (
                                      Session["memberTradeAccountID"] != null
                                       && Convert.ToBoolean(System.Web.HttpContext.Current.Session["tradeOrderingAllowed"])
                                       && Convert.ToBoolean(System.Web.HttpContext.Current.Session["memberTradeChecksPassed"])
                                  ) == false
                               )
                            {
                                var redirectUrl = dgFramework.buildUrl(
                                    dgContentType.landingPage,
                                    (int)dgFixedPage.home,
                                    urlData.countryDetails,
                                    urlData.ContentId,
                                    dgContentType.landingPage,
                                    null,
                                    null,
                                    Framework.ProductionVersion ? null : new List<string>() { "dateOverride", "countryOverride" }
                                );

                                // Don't index this URL: it is login protected
                                Response.Headers.Add("X-Robots-Tag", "noindex");

                                return new RedirectResult(redirectUrl, false);
                            }
                        }
                    }
                }

                var landingPageViewModel = await LandingPageViewModel.CreateAsync(
                    objMaster: await NewMasterDefinition.CreateAsync(
                        titleTag: null,
                        metaDescription: null,
                        backInStockReminder: (int?)Session["backInStockReminder"],
                        itemsInBasket: 0m,
                        urlBreakdown: urlData,
                        googleHrefs: null,
                        tradeMember: Session["memberTradeAccountID"] != null
                    ),
                    objVisitor: objVisitorSelect,
                    isShop: IsShop
                );

                if (landingPageViewModel.Definition.PageDetails.EnableFilterIndexing)
                {
                    landingPageViewModel.OriginalSemanticString = landingPageViewModel.Definition.PageDetails.URLKeywords?.Split(Framework.ForwardSlash).LastOrDefault();

                    landingPageViewModel.PageSections.ForEach(ps =>
                    {
                        if (ps.Definition is ProductPageGridPageSection pg)
                        {
                            pg.OriginalSemanticString = landingPageViewModel.OriginalSemanticString;
                        }
                    });
                }

                if (String.IsNullOrEmpty(urlParameter) &&
                    urlData.ContentId == (int)dgFixedPage.globalSiteSelector &&
                    urlData.ContentType == dgContentType.landingPage)
                {
                    // visiting the global site selector, don't need to redirect anywhere or stick a country on the end so skip that noise
                }
                else
                {
                    if (landingPageViewModel.Definition.PageDetails.EnableFilterIndexing == false &&
                        URLKeywordsRequireCorrecting(urlData.countryDetails.URL, landingPageViewModel.Definition.PageDetails.URLKeywords, urlParameterWithoutSuffix) == true)
                    {
                        //  Self-healing URL (note: dateOverride is built up after url)
                        var tempUrl = dgFramework.buildUrl(
                            urlData.countryDetails,
                            landingPageViewModel.Definition.PageDetails.URLKeywords,
                            dgContentType.landingPage,
                            urlData.ContentId,
                            !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                            !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                            Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                        // 03/04/2023: Scratch that: apparently the noindex also means "don't register the redirect"

                        // 03/04/2023: Amine has told us that he wants to see these URLKeyword correcting redirects 
                        // become 301's instead of 302's, but with a cache-control: nocache
                        Response.Headers.Add("cache-control", "no-cache");

                        return new RedirectResult(tempUrl, true);
                    }

                    var filterIndexingURLKeywordsCorrection = new Tuple<string, NameValueCollection>(null, null);

                    if (landingPageViewModel.Definition.PageDetails.EnableFilterIndexing)
                    {
                        filterIndexingURLKeywordsCorrection = await FilterIndexingURLKeywordsRequireCorrecting(
                                urlData.countryDetails.URL,
                                landingPageViewModel.Definition.PageDetails.URLKeywords,
                                urlParameterWithoutSuffix,
                                urlData.countryDetails.LanguageID,
                                landingPageViewModel.Definition.PageDetails.ProductTypeID,
                                landingPageViewModel);
                    }

                    if (!String.IsNullOrEmpty(filterIndexingURLKeywordsCorrection.Item1))
                    {
#if DEBUG
                        System.Diagnostics.Debug.WriteLine($"filterIndexingURLKeywordsCorrection:\r\n\t\t{urlData.SemanticString}\r\nto\t\t{landingPageViewModel.Definition.PageDetails.URLKeywords}");
#endif

                        foreach(var k in Request.QueryString.AllKeys)
                        {
                            filterIndexingURLKeywordsCorrection.Item2.Add(k, Request.QueryString[k]);
                        }

                        //  Self-healing URL (note: dateOverride is built up after url)
                        var tempUrl = dgFramework.buildUrl(
                            urlData.countryDetails,
                            filterIndexingURLKeywordsCorrection.Item1.StartsWith($"{urlData.countryDetails.URL}/") 
                                ? filterIndexingURLKeywordsCorrection.Item1.Substring($"{urlData.countryDetails.URL}/".Length)
                                : filterIndexingURLKeywordsCorrection.Item1,
                            dgContentType.landingPage,
                            urlData.ContentId,
                            !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                            !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                            filterIndexingURLKeywordsCorrection.Item2.HasKeys() ? $"{filterIndexingURLKeywordsCorrection.Item2}" : null);

                        // 03/04/2023: Scratch that: apparently the noindex also means "don't register the redirect"

                        // 03/04/2023: Amine has told us that he wants to see these URLKeyword correcting redirects 
                        // become 301's instead of 302's, but with a cache-control: nocache
                        Response.Headers.Add("cache-control", "no-cache");

                        return new RedirectResult(tempUrl, true);
                    }

                    if (landingPageViewModel.Definition.PageDetails.EnableFilterIndexing &&
                        landingPageViewModel
                            .PageSections
                            .Where(ps => ps.PageSectionType == PageSectionType.ProductPageGrid)
                            .Any(ps => !String.IsNullOrEmpty(((ProductPageGridPageSection)ps.Definition).DeselectedAttributeNames)))
                    {
                        var ds = (landingPageViewModel
                            .PageSections
                            .Where(ps => ps.PageSectionType == PageSectionType.ProductPageGrid)
                            .First(ps => !String.IsNullOrEmpty(((ProductPageGridPageSection)ps.Definition).DeselectedAttributeNames))
                            .Definition as ProductPageGridPageSection).DeselectedAttributeNames;

                        Session["productPageGridDeselectedAttributesNotice"] = ds;

                        //  Self-healing URL (note: dateOverride is built up after url)
                        var tempUrl = dgFramework.buildUrl(
                            urlData.countryDetails,
                            landingPageViewModel.Definition.PageDetails.URLKeywords,
                            dgContentType.landingPage,
                            urlData.ContentId,
                            !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                            !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                            Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);


                        // Don't index the approached URL: it required correction
                        // Response.Headers.Add("X-Robots-Tag", "noindex");
                        //
                        // 03/04/2023: Scratch that: apparently the noindex also means "don't register the redirect"

                        // 03/04/2023: Amine has told us that he wants to see these URLKeyword correcting redirects 
                        // become 301's instead of 302's, but with a cache-control: nocache
                        Response.Headers.Add("cache-control", "no-cache");

                        return new RedirectResult(tempUrl, true);
                    }
                }

                if (landingPageViewModel.ContentID == (int)dgFixedPage.memberLogin)
                {
                    Response.Headers.Add("X-Robots-Tag", "noindex");
                }

                var pageView = new logPageView()
                {
                    Site = Framework.ProductionVersion ? 'L' : 'P',
                    URL = Request.Url.AbsoluteUri,
                    CountryID = urlData.countryDetails.CountryID,
                    LanguageID = urlData.countryDetails.LanguageID,
                    PageID = urlData.ContentId,
                    Date = urlData.ContentDate,
                    VisitorID = objVisitorSelect.visitorDbId,
                    MemberID = objVisitorSelect.memberId,
                    TradeAccountID = objVisitorSelect.memberTradeAccountID,
                    UserAgent = Request.UserAgent,
                    CFIPCountry = Request.Headers["CF-IPCountry"],
                    CloudFlareDevice = Request.Headers["CF-Device-Type"],
                    SessionID = Session.SessionID,
                    GoogleSource = Request.QueryString["utm_source"],
                    GoogleMedium = Request.QueryString["utm_medium"],
                    GoogleCampaign = Request.QueryString["utm_campaign"],
                    GoogleTerm = Request.QueryString["utm_term"],
                    GoogleContent = Request.QueryString["utm_content"]
                };

                objVisitorFactory.logPageView(pageView);

                if (landingPageViewModel.ContentID == (int)dgFixedPage.globalSiteSelector)
                {
                    var viewModel = await FixedPageViewModel_GlobalSiteSelector.CreateAsync(Request.Headers["CF-IPCountry"], landingPageViewModel);

                    return View("~/Views-New/LandingPages/FixedPages/_GlobalSiteSelector.cshtml", viewModel);
                }
                else
                {
                    return View("~/Views-New/LandingPages/LandingPage.cshtml", landingPageViewModel);
                }
            }
            #endregion


            #region Build CollectionPage

            if (urlData.ContentType == dgContentType.collectionPage)
            {
                var objCollectionPage = new cmsCollectionPage();

                // Can we take our URLParameter's ContentId and fit it into a Short?
                if (short.TryParse(urlData.ContentId.ToString(), out short collectionID)) { }
                else
                {
                    // If not, redirect as appropriate
                    //  Redirect the UK home page (last fall back)
                    string url = dgFramework.buildUrl(urlData.countryDetails,
                                                    null,
                                                    DefaultContentType,
                                                    DefaultContentId,
                                                    !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                    !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                    Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(url, true);
                }


                // We have got a valid Short out of our URLParameter ContentId, try to get a collection using our data.

                int? memberTradeAccountId = null;

                if (Session["memberTradeAccountID"] != null)
                {
                    memberTradeAccountId = (int?)Session["memberTradeAccountID"];
                }

                var objBasket = new basketRoutines();

                var basketTotalData = await objBasket.loadBasketTotalQuantity(objVisitorSelect.visitorDbId, memberId, urlData.countryDetails.CountryID);

                var collectionModel = await CollectionViewModel.CreateAsync
                    (
                        objMaster: await NewMasterDefinition.CreateAsync(
                            titleTag: null,//objCollectionData.collectionSelects[0].TitleTag,
                            metaDescription: null,//objCollectionData.collectionSelects[0].MetaDescription,
                            backInStockReminder: (int?)Session["backInStockReminder"],
                            itemsInBasket: basketTotalData.TotalQuantity,
                            urlBreakdown: urlData,
                            googleHrefs: null,// objMaster.googleHrefs,
                            tradeMember: Session["memberTradeAccountID"] != null
                        ),
                        objVisitor: objVisitorSelect
                    );

                // We have no collection selected: redirect as appropriate.
                if (collectionModel.Definition.CollectionDetails == null)
                {
                    //  Redirect the UK home page (last fall back)
                    string url = dgFramework.buildUrl(urlData.countryDetails,
                                                    null,
                                                    DefaultContentType,
                                                    DefaultContentId,
                                                    !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                    !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                    Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    // Don't index the approached URL: it was an invalid or unavailable collection
                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(url, true);
                }
                else
                {
                    collectionModel.MasterDefinition.TitleTag = collectionModel.Definition.CollectionDetails.TitleTag;
                    collectionModel.MasterDefinition.MetaDescription = collectionModel.Definition.CollectionDetails.MetaDescription;
                    collectionModel.MasterDefinition.GoogleHrefs = collectionModel.Definition.collectionGoogleHrefs;
                }

                if (URLKeywordsRequireCorrecting(urlData.countryDetails.URL, collectionModel.Definition.CollectionDetails.URLKeywords, urlParameterWithoutSuffix) == true)
                {
                    //  Self-healing URL
                    string tempUrl = dgFramework.buildUrl(urlData.countryDetails, collectionModel.Definition.CollectionDetails.URLKeywords, dgContentType.collectionPage, urlData.ContentId,
                                                                !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                                !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                                Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(tempUrl, true);
                }
                ////////////
                //
                // SERVE No. 4
                // (not a redirect)
                //

                #region LOG PAGEVIEW

                #endregion

                await FetchBanners(collectionModel.MasterDefinition, null);

                var pageView = new logPageView()
                {
                    Site = Framework.ProductionVersion ? 'L' : 'P',
                    URL = Request.Url.AbsoluteUri,
                    CountryID = urlData.countryDetails.CountryID,
                    LanguageID = urlData.countryDetails.LanguageID,
                    CollectionID = (short?)urlData.ContentId,
                    Date = urlData.ContentDate,
                    VisitorID = objVisitorSelect.visitorDbId,
                    MemberID = objVisitorSelect.memberId,
                    TradeAccountID = objVisitorSelect.memberTradeAccountID,
                    UserAgent = Request.UserAgent,
                    CFIPCountry = Request.Headers["CF-IPCountry"],
                    CloudFlareDevice = Request.Headers["CF-Device-Type"],
                    SessionID = Session.SessionID,
                    GoogleSource = Request.QueryString["utm_source"],
                    GoogleMedium = Request.QueryString["utm_medium"],
                    GoogleCampaign = Request.QueryString["utm_campaign"],
                    GoogleTerm = Request.QueryString["utm_term"],
                    GoogleContent = Request.QueryString["utm_content"]
                };

                objVisitorFactory.logPageView(pageView);

                return View("~/Views-New/Collections/Collection.cshtml", collectionModel);
            }

            #endregion


            #region Build ProductPage


            if (urlData.ContentType == dgContentType.productPage)
            {
                var objProductPage = new cmsProductPage();

                var objProductData = await objProductPage.loadProductPageFromCms(
                        urlData.ContentId,
                        urlData.countryDetails.LanguageID,
                        urlData.countryDetails.CountryID,
                        urlData.ContentDate,
                        Session["memberTradeAccountID"] != null);

                if (objProductData.objCacheSelect == null)
                {
                    if (objProductData.objUnavailabilityRedirect == null || objProductData.objUnavailabilityRedirect.redirectUrl == null)
                    {
                        //  Redirect the UK home page (last fall back)
                        var url = dgFramework.buildUrl(urlData.countryDetails,
                                                        null,
                                                        DefaultContentType,
                                                        DefaultContentId,
                                                        !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                        !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                        Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                        Response.Headers.Add("cache-control", "no-cache");

                        return new RedirectResult(url, true);
                    }
                    else
                    {
                        Response.Headers.Add("cache-control", "no-cache");

                        return new RedirectResult(objProductData.objUnavailabilityRedirect.redirectUrl, true);
                    }
                }


                if (URLKeywordsRequireCorrecting(urlData.countryDetails.URL, objProductData.objCacheSelect.urlSemantics, urlParameterWithoutSuffix) == true)
                {
                    //  Self-healing URL
                    var tempUrl = dgFramework.buildUrl(urlData.countryDetails, objProductData.objCacheSelect.urlSemantics, dgContentType.productPage, urlData.ContentId,
                                                                !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                                !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                                Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(tempUrl, true);
                }

                var objBasket = new basketRoutines();

                var basketTotalData = await objBasket.loadBasketTotalQuantity(objVisitorSelect.visitorDbId, memberId, urlData.countryDetails.CountryID);

                //#if DEBUG
                //              Response.Headers.Add("Content-Security-Policy", "default-src 'unsafe-inline' http://localhost:* ws://localhost:* 'self'; script-src 'unsafe-inline' http://localhost:* 'self' 'unsafe-eval' https://static.trackedweb.net/js/_dmptv4.js https://connect.facebook.net/en_US/sdk.js https://assets.pinterest.com:* https://www.googletagmanager.com:* https://www.google-analytics.com:* https://salesiq.zoho.eu:* https://salesiq.zoho.com:* https://js.zohocdn.com:*; img-src https://site.feefo.com https://www.designersguild.com https://www.facebook.com https://log.pinterest.com/ https://www.google-analytics.com/ 'self' data:; style-src 'unsafe-inline' 'self' https://css.zohostatic.eu/ https://css.zohostatic.com/ https://css.zohocdn.com/; font-src 'self' https://css.zohocdn.com; connect-src 'self' https://salesiq.zoho.eu/ https://salesiq.zoho.com/ wss://vts.zohopublic.eu/ https://r1.trackedweb.net/ https://localhost:* wss://localhost:*");
                //#else
                //              Response.Headers.Add("Content-Security-Policy", "default-src 'unsafe-inline' http://localhost:* ws://localhost:* 'self'; script-src 'unsafe-inline' http://localhost:* 'self' 'unsafe-eval' https://static.trackedweb.net/js/_dmptv4.js https://connect.facebook.net/en_US/sdk.js https://assets.pinterest.com:* https://www.googletagmanager.com:* https://www.google-analytics.com:* https://salesiq.zoho.eu:* https://salesiq.zoho.com:* https://js.zohocdn.com:*; img-src https://site.feefo.com https://www.designersguild.com https://www.facebook.com https://log.pinterest.com/ https://www.google-analytics.com/ 'self' data:; style-src 'unsafe-inline' 'self' https://css.zohostatic.eu/ https://css.zohostatic.com/ https://css.zohocdn.com/; font-src 'self' https://css.zohocdn.com; connect-src 'self' https://salesiq.zoho.eu/ https://salesiq.zoho.com/ wss://vts.zohopublic.com/ https://r1.trackedweb.net/ https://localhost:* wss://localhost:*");
                //#endif

                var pageView = new logPageView()
                {
                    Site = Framework.ProductionVersion ? 'L' : 'P',
                    URL = Request.Url.AbsoluteUri,
                    CountryID = urlData.countryDetails.CountryID,
                    LanguageID = urlData.countryDetails.LanguageID,
                    ProductPageID = urlData.ContentId,
                    Date = urlData.ContentDate,
                    VisitorID = objVisitorSelect.visitorDbId,
                    MemberID = objVisitorSelect.memberId,
                    TradeAccountID = objVisitorSelect.memberTradeAccountID,
                    UserAgent = Request.UserAgent,
                    CFIPCountry = Request.Headers["CF-IPCountry"],
                    CloudFlareDevice = Request.Headers["CF-Device-Type"],
                    SessionID = Session.SessionID,
                    GoogleSource = Request.QueryString["utm_source"],
                    GoogleMedium = Request.QueryString["utm_medium"],
                    GoogleCampaign = Request.QueryString["utm_campaign"],
                    GoogleTerm = Request.QueryString["utm_term"],
                    GoogleContent = Request.QueryString["utm_content"]
                };

                objVisitorFactory.logPageView(pageView);

                // All cases in here SERVE [No. 4, whatever that means in 2022 Tom?] instead of redirecting
                switch (objProductData.objCacheSelect.templateId)
                {
                    case 2: // Fabric and Wallpaper
                        var fabricWallpaperModel = await ProductPageViewModel_FabricWallpaper.CreateAsync
                        (
                            objMaster: await NewMasterDefinition.CreateAsync(
                                titleTag: objProductData.objCacheSelect.titleTag,
                                metaDescription: objProductData.objCacheSelect.metaDescription,
                                backInStockReminder: (int?)Session["backInStockReminder"],
                                itemsInBasket: basketTotalData.TotalQuantity,
                                urlBreakdown: urlData,
                                googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                tradeMember: Session["memberTradeAccountID"] != null
                            ),
                            objVisitor: objVisitorSelect,
                            objCacheSelect: objProductData.objCacheSelect,
                            isShop: IsShop
                        );

                        await FetchBanners(fabricWallpaperModel.MasterDefinition, null);

                        return View(dgFramework.GetPreferredDeviceView("~/Views-New/ProductPages/FabricWallpaper.cshtml"), fabricWallpaperModel);

                    case 3: // Shop
                        var shopModel = await ProductPageViewModel_Standard.CreateAsync
                            (
                                objMaster: await NewMasterDefinition.CreateAsync(
                                    titleTag: objProductData.objCacheSelect.titleTag,
                                    metaDescription: objProductData.objCacheSelect.metaDescription,
                                    backInStockReminder: (int?)Session["backInStockReminder"],
                                    itemsInBasket: basketTotalData.TotalQuantity,
                                    urlBreakdown: urlData,
                                    googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                    tradeMember: Session["memberTradeAccountID"] != null
                                ),
                                objVisitor: objVisitorSelect,
                                objCacheSelect: objProductData.objCacheSelect,
                                isShop: IsShop
                            );

                        await FetchBanners(shopModel.MasterDefinition, null);

                        return View(dgFramework.GetPreferredDeviceView("~/Views-New/ProductPages/Standard.cshtml"), shopModel);

                    case 4: // Furniture
                        var furnitureModel = await ProductPageViewModel_Furniture_Wholesale.CreateAsync
                            (
                                objMaster: await NewMasterDefinition.CreateAsync(
                                    titleTag: objProductData.objCacheSelect.titleTag,
                                    metaDescription: objProductData.objCacheSelect.metaDescription,
                                    backInStockReminder: (int?)Session["backInStockReminder"],
                                    itemsInBasket: basketTotalData.TotalQuantity,
                                    urlBreakdown: urlData,
                                    googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                    tradeMember: Session["memberTradeAccountID"] != null
                                ),
                                objVisitor: objVisitorSelect,
                                objCacheSelect: objProductData.objCacheSelect,
                                isShop: IsShop
                            );

                        await FetchBanners(furnitureModel.MasterDefinition, null);

                        return View(dgFramework.GetPreferredDeviceView("~/Views-New/ProductPages/Furniture-Wholesale.cshtml"), furnitureModel);

                    case 5: // Modular Product Page
                        var modularModel = await ProductPageViewModel_Furniture_Modular.CreateAsync
                            (
                                objMaster: await NewMasterDefinition.CreateAsync(
                                    titleTag: objProductData.objCacheSelect.titleTag,
                                    metaDescription: objProductData.objCacheSelect.metaDescription,
                                    backInStockReminder: (int?)Session["backInStockReminder"],
                                    itemsInBasket: basketTotalData.TotalQuantity,
                                    urlBreakdown: urlData,
                                    googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                    tradeMember: Session["memberTradeAccountID"] != null
                                ),
                                objVisitor: objVisitorSelect,
                                objCacheSelect: objProductData.objCacheSelect,
                                isShop: IsShop
                            );

                        await FetchBanners(modularModel.MasterDefinition, null);

                        return View("~/Views-New/ProductPages/Furniture-Modular.cshtml", modularModel);

                    case 6: // Consolidated Order Form
                        var consolidatedOrderFormViewModel = await ProductPageViewModel_Standard.CreateAsync
                            (
                                objMaster: await NewMasterDefinition.CreateAsync(
                                    titleTag: objProductData.objCacheSelect.titleTag,
                                    metaDescription: objProductData.objCacheSelect.metaDescription,
                                    backInStockReminder: (int?)Session["backInStockReminder"],
                                    itemsInBasket: basketTotalData.TotalQuantity,
                                    urlBreakdown: urlData,
                                    googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                    tradeMember: Session["memberTradeAccountID"] != null
                                ),
                                objVisitor: objVisitorSelect,
                                objCacheSelect: objProductData.objCacheSelect,
                                isShop: IsShop
                            );

                        consolidatedOrderFormViewModel.UseOrderFormGrouping = true;
                        consolidatedOrderFormViewModel.UseSizeButtons = true;

                        await FetchBanners(consolidatedOrderFormViewModel.MasterDefinition, null);

                        return View("~/Views-New/ProductPages/Standard.cshtml", consolidatedOrderFormViewModel);

                    case 9:
                        // Paint  
                        var paintModel = await ProductPageViewModel_Paint.CreateAsync
                        (
                            objMaster: await NewMasterDefinition.CreateAsync(
                                titleTag: objProductData.objCacheSelect.titleTag,
                                metaDescription: objProductData.objCacheSelect.metaDescription,
                                    backInStockReminder: (int?)Session["backInStockReminder"],
                                itemsInBasket: basketTotalData.TotalQuantity,
                                urlBreakdown: urlData,
                                googleHrefs: objProductData.objCacheSelect.googleHrefs,
                                tradeMember: Session["memberTradeAccountID"] != null
                            ),
                            objVisitor: objVisitorSelect,
                            objCacheSelect: objProductData.objCacheSelect,
                            isShop: IsShop
                        );

                        await FetchBanners(paintModel.MasterDefinition, null);

                        return View("~/Views-New/ProductPages/Paint.cshtml", paintModel);
                        
                    default:
                        throw new Exception("Unrecognised TemplateID!");
                }
            }
#endregion


            #region Build ArticlePage
            
            if (urlData.ContentType == dgContentType.articlePage)
            {
                var objLandingPage = new cmsArticlePage();
                var objArticleData = await objLandingPage.loadArticlePageFromCms(urlData.ContentId,
                                                                                        urlData.countryDetails.LanguageID,
                                                                                        urlData.countryDetails.CountryID,
                                                                                        urlData.ContentDate);

                if (objArticleData.ArticleDetails == null)
                {
                    //  Redirect the UK home page (last fall back)
                    string url = dgFramework.buildUrl(urlData.countryDetails,
                                                    null,
                                                    DefaultContentType,
                                                    DefaultContentId,
                                                    !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                    !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                    Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(url, true);
                }


                if (URLKeywordsRequireCorrecting(urlData.countryDetails.URL, objArticleData.ArticleDetails.URLKeywords, urlParameterWithoutSuffix) == true)
                {
                    //  Self-healing URL (note: dateOverride is built up after url)
                    string tempUrl = dgFramework.buildUrl(urlData.countryDetails, objArticleData.ArticleDetails.URLKeywords, dgContentType.articlePage, urlData.ContentId,
                                                        !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                        !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                        Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(tempUrl, true);
                }

                var objBasket = new basketRoutines();

                var basketTotalData = await objBasket.loadBasketTotalQuantity(objVisitorSelect.visitorDbId, memberId, urlData.countryDetails.CountryID);

                var articleViewModel = await ArticleViewModel.CreateAsync
                (
                    objMaster: await NewMasterDefinition.CreateAsync(
                        titleTag: objArticleData.ArticleDetails.TitleTag,
                        metaDescription: objArticleData.ArticleDetails.MetaDescription,
                        backInStockReminder: (int?)Session["backInStockReminder"],
                        itemsInBasket: basketTotalData.TotalQuantity,
                        urlBreakdown: urlData,
                        googleHrefs: objArticleData.articleGoogleHrefs,
                        tradeMember: Session["memberTradeAccountID"] != null
                    ),
                    objVisitor: objVisitorSelect,
                    objDefinition: objArticleData
                );

                var pageView = new logPageView()
                {
                    Site = Framework.ProductionVersion ? 'L' : 'P',
                    URL = Request.Url.AbsoluteUri,
                    CountryID = urlData.countryDetails.CountryID,
                    LanguageID = urlData.countryDetails.LanguageID,
                    ArticleID = urlData.ContentId,
                    Date = urlData.ContentDate,
                    VisitorID = objVisitorSelect.visitorDbId,
                    MemberID = objVisitorSelect.memberId,
                    TradeAccountID = objVisitorSelect.memberTradeAccountID,
                    UserAgent = Request.UserAgent,
                    CFIPCountry = Request.Headers["CF-IPCountry"],
                    CloudFlareDevice = Request.Headers["CF-Device-Type"],
                    SessionID = Session.SessionID,
                    GoogleSource = Request.QueryString["utm_source"],
                    GoogleMedium = Request.QueryString["utm_medium"],
                    GoogleCampaign = Request.QueryString["utm_campaign"],
                    GoogleTerm = Request.QueryString["utm_term"],
                    GoogleContent = Request.QueryString["utm_content"]
                };

                objVisitorFactory.logPageView(pageView);

                return View("~/Views-New/Articles/Article.cshtml", articleViewModel);
            }

            #endregion


            #region Build CheckoutPart

            if (urlData.ContentType == dgContentType.checkoutPart)
            {
                if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
                {
                    //  Redirect the home page (last fall back)
                    string url = dgFramework.buildUrl(urlData.countryDetails,
                                                    null,
                                                    DefaultContentType,
                                                    DefaultContentId,
                                                    !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                    !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                    Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    // Don't index the approached URL: it's part of the checkout
                    Response.Headers.Add("X-Robots-Tag", "noindex");

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(url, true);
                }
                else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
                {
                    //  Redirect the home page (last fall back)
                    string url = dgFramework.buildUrl(urlData.countryDetails,
                                                        null,
                                                        DefaultContentType,
                                                        DefaultContentId,
                                                        !Framework.ProductionVersion ? Request.QueryString["dateOverride"] : null,
                                                        !Framework.ProductionVersion ? Request.QueryString["countryOverride"] : null,
                                                        Request.QueryString.AllKeys.Any() ? $"{Request.QueryString}" : null);

                    // Don't index the approached URL: it's part of the checkout
                    Response.Headers.Add("X-Robots-Tag", "noindex");

                    Response.Headers.Add("cache-control", "no-cache");

                    return new RedirectResult(url, true);
                }

                var checkoutPartViewModel = await LandingPageViewModel.CreateAsync(
                    objMaster: await NewMasterDefinition.CreateAsync(
                        titleTag: null,
                        metaDescription: null,
                        backInStockReminder: (int?)Session["backInStockReminder"],
                        itemsInBasket: 0m,
                        urlBreakdown: urlData,
                        googleHrefs: null,
                        tradeMember: Session["memberTradeAccountID"] != null
                    ),
                    objVisitor: objVisitorSelect,
                    isShop: IsShop
                );

                // Don't index the approached URL: it's part of the checkout
                Response.Headers.Add("X-Robots-Tag", "noindex");

                var result = View("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutPart.cshtml", checkoutPartViewModel);

                return result;
            }

            #endregion CheckoutPart

            #region End Return 404 (never hit)

            return HttpNotFound();

            #endregion
        }

        
        #region Identification Routines

        private async Task<string> getIdentificationCountry(Int64 dbIdentificationId)
        {
            VisitorFactory visitorFactory = new VisitorFactory();
            return await visitorFactory.callVisitorSelectCulture(dbIdentificationId);
        }


        private async Task<Boolean> updateVisitor(Int64 dbIdentificationId, string ipAddress, string urlCountry, string cloudFlareCountryCode, string cloudFlareDevice)
        {
            VisitorFactory visitorFactory = new VisitorFactory();
            return await visitorFactory.callVisitorUpdate(dbIdentificationId, ipAddress, urlCountry, cloudFlareCountryCode, cloudFlareDevice);
        }

        #endregion

        // the beneath implementation does not permit a URL to fail to contain a site e.g.  /uk/l103 is fine but /l103 will drop you back to /uk/l0 instead of healing to /uk/l103
        //private async Task<urlBreakdown> buildUrlBreakdown(string urlParameter, cmsCountryDefinition objCountryData, VisitorSelect objVisitorSelect, DateTime useDate)
        //{
        //    //  Initialise to defaults
        //    var result = new urlBreakdown
        //    {
        //        countryDetails = dgFramework.getCountryFromText(dgFramework.getDefaultCountry(), objCountryData),
        //        ContentType = dgContentType.landingPage,
        //        ContentId = (int)dgFixedPage.globalSiteSelector,
        //        SemanticString = "",
        //        ContentDate = useDate,
        //        contentTypeAndIdNotFound = false
        //    };

        //    if (String.IsNullOrEmpty(urlParameter))
        //    {
        //        // this isn't the be-all-and-end-all, is it
        //        // because if the visitor has a previous culture, we want to direct them to that rather than serve them the www.designersguild.com page
        //        // see lines 279 - 292
        //        return result;
        //    }

        //    var validSiteDetected = false;

        //    var parameters = urlParameter.Split('/');

        //    if (parameters.Length > 0)
        //    {
        //        if (dgFramework.doesCountryExistFromText(parameters[0], objCountryData) == true)
        //        {
        //            result.countryDetails = dgFramework.getCountryFromText(parameters[0], objCountryData);
        //            validSiteDetected = true;
        //        }
        //        else
        //        {
        //            //we have not found a country based on the first segment: we MUST redirect.
        //            //result.contentTypeAndIdNotFound = true;

        //            //reinstate the ability for redirect-to-culture (first from cookie then from IP) if a valid type and id are found
        //            //

        //            if ((objVisitorSelect.Culture == "") || (objVisitorSelect.Culture == null))
        //            {
        //                // [B-i-a]
        //                // get the default country (UK)
        //                string defaultGeoCountry = dgFramework.getDefaultCountry();
        //                bool gotCountryFromCloudFlare = false;

        //                if (Request.Headers["CF-IPCountry"] == null)
        //                {
        //                    // This will be null locally. In Production, and on published Dev and Preview sites, CloudFlare guarantee us this header.
        //                }
        //                else
        //                {
        //                    string cfIPCountryCode = Request.Headers["CF-IPCountry"];
        //                    _ = String.Format("Got CountryCode from CloudFlare: {0} ", cfIPCountryCode);

        //                    if (dgFramework.doesCountryExistFromCode(cfIPCountryCode, objCountryData))
        //                    {
        //                        result.countryDetails = dgFramework.getCountryFromCode(cfIPCountryCode, objCountryData);
        //                        gotCountryFromCloudFlare = true;
        //                    }
        //                    else
        //                    {
        //                    }

        //                    //Logger.Trace(cfTrace);
        //                }

        //                // [B-i-c]
        //                // Does a country exist in our Country_Language list by name of defaultGeoCountry
        //                if (!gotCountryFromCloudFlare)
        //                {
        //                    if (dgFramework.doesCountryExistFromText(defaultGeoCountry, objCountryData) == true)
        //                    {
        //                        result.countryDetails = dgFramework.getCountryFromText(defaultGeoCountry, objCountryData);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                // [B-i-b]
        //                // sanity check: we should never store an invalid culture on the visitor
        //                if (dgFramework.doesCountryExistFromText(objVisitorSelect.Culture, objCountryData) == true)
        //                {
        //                    result.countryDetails = dgFramework.getCountryFromText(objVisitorSelect.Culture, objCountryData);
        //                }
        //            }
        //        }

        //        if (parameters.Length >= 2) // we have at least a one/two urlParameter scenario
        //        {
        //            string source = parameters[parameters.Length - 1];

        //            if (source.Length > 1)
        //            {
        //                string firstCharacter = source.Substring(0, 1);
        //                string otherCharacters = source.Substring(1);

        //                //  Content type
        //                result.ContentType = dgFramework.getContentTypeFromString(firstCharacter);

        //                if (result.ContentType == dgContentType.invalid)
        //                {
        //                    result.contentTypeAndIdNotFound = true;
        //                    return result;
        //                }

        //                //  Content id
        //                if (Int32.TryParse(otherCharacters, out int tempContentId))
        //                {
        //                    result.ContentId = tempContentId;
        //                }
        //                else
        //                {
        //                    result.contentTypeAndIdNotFound = true;
        //                    return result;
        //                }

        //                //  Semantic String
        //                var tempSemanticString = "";

        //                for (int paramCount = 1; paramCount < parameters.Length - 1; paramCount++)
        //                {
        //                    tempSemanticString = String.Format("{0}/{1}", tempSemanticString, parameters[paramCount]);
        //                }

        //                result.SemanticString = HttpUtility.UrlDecode(tempSemanticString.TrimStart(new char[] { '/' }));
        //            }
        //            else
        //            {
        //                result.contentTypeAndIdNotFound = true;
        //                return result;
        //            }
        //        }
        //        else // we have precisely one urlSegment
        //        {
        //            if (validSiteDetected) // if we detected that that segment was a valid site
        //            {   // drop the user to the GHP for that site
        //                result.ContentId = (int)dgFixedPage.home;
        //                result.ContentType = dgContentType.landingPage;
        //                result.contentTypeAndIdNotFound = true;
        //            }
        //            else
        //            {
        //                result.contentTypeAndIdNotFound = true;
        //            }

        //            return result;
        //        }
        //    }

        //    return result;
        //}

        private async Task<urlBreakdown> buildUrlBreakdown(string urlParameter, cmsCountryDefinition objCountryData, VisitorSelect objVisitorSelect, DateTime useDate)
        {
            //  Initialise to defaults
            var result = new urlBreakdown
            {
                countryDetails = dgFramework.getCountryFromText(dgFramework.getDefaultCountry(), objCountryData),
                ContentType = dgContentType.landingPage,
                ContentId = (int)dgFixedPage.globalSiteSelector,
                SemanticString = "",
                ContentDate = useDate,
                siteCountryNotFoundInUrl = false,
                contentTypeAndIdNotFound = false
            };

            if (String.IsNullOrEmpty(urlParameter))
            {
                // this isn't the be-all-and-end-all, is it
                // because if the visitor has a previous culture, we want to direct them to that rather than serve them the www.designersguild.com page
                // see lines 279 - 292
                return result;
            }

            var parameters = urlParameter.Split('/');

            if (parameters.Length > 0)
            {
                if (dgFramework.doesCountryExistFromText(parameters[0], objCountryData) == true)
                {
                    result.countryDetails = dgFramework.getCountryFromText(parameters[0], objCountryData);

                }
                else
                {
                    result.siteCountryNotFoundInUrl = true;
                    
                    if ((objVisitorSelect.Culture == "") || (objVisitorSelect.Culture == null))
                    {
                        // [B-i-a]
                        // get the default country (UK)
                        string defaultGeoCountry = dgFramework.getDefaultCountry();
                        bool gotCountryFromCloudFlare = false;

                        if (Request.Headers["CF-IPCountry"] == null)
                        {
                            // This will be null locally. In Production, and on published Dev and Preview sites, CloudFlare guarantee us this header.
                        }
                        else
                        {
                            string cfIPCountryCode = Request.Headers["CF-IPCountry"];
                            _ = String.Format("Got CountryCode from CloudFlare: {0} ", cfIPCountryCode);

                            if (dgFramework.doesCountryExistFromCode(cfIPCountryCode, objCountryData))
                            {
                                result.countryDetails = dgFramework.getCountryFromCode(cfIPCountryCode, objCountryData);
                                gotCountryFromCloudFlare = true;
                            }
                            else
                            {
                            }

                            //Logger.Trace(cfTrace);
                        }

                        // [B-i-c]
                        // Does a country exist in our Country_Language list by name of defaultGeoCountry
                        if (!gotCountryFromCloudFlare)
                        {
                            if (dgFramework.doesCountryExistFromText(defaultGeoCountry, objCountryData) == true)
                            {
                                result.countryDetails = dgFramework.getCountryFromText(defaultGeoCountry, objCountryData);
                            }
                        }
                    }
                    else
                    {
                        // [B-i-b]
                        if (dgFramework.doesCountryExistFromText(objVisitorSelect.Culture, objCountryData) == true)
                        {
                            result.countryDetails = dgFramework.getCountryFromText(objVisitorSelect.Culture, objCountryData);
                        }
                    }
                }

                string source = parameters[parameters.Length - 1];

                if (source.Length > 1)
                {
                    string firstCharacter = source.Substring(0, 1);
                    string otherCharacters = source.Substring(1);

                    //  Content type
                    result.ContentType = dgFramework.getContentTypeFromString(firstCharacter);

                    if (result.ContentType == dgContentType.invalid)
                    {
                        result.contentTypeAndIdNotFound = true;
                        return result;
                    }

                    //  Content id
                    if (Int32.TryParse(otherCharacters, out int tempContentId))
                    {
                        result.ContentId = tempContentId;
                    }
                    else
                    {
                        result.contentTypeAndIdNotFound = true;
                        return result;
                    }

                    //  Semantic String
                    var tempSemanticString = "";

                    for (int paramCount = 1; paramCount < parameters.Length - 1; paramCount++)
                    {
                        tempSemanticString = String.Format("{0}/{1}", tempSemanticString, parameters[paramCount]);
                    }

                    result.SemanticString = tempSemanticString.TrimStart(new char[] { '/' });
                }
                else
                {
                    result.contentTypeAndIdNotFound = true;
                    return result;
                }
            }

            return result;
        }

        private async Task FetchBanners(NewMasterDefinition objMaster, byte? siteSectionId)
        {
            var banners = new cmsBanner();

            var bannerStore = await banners.loadBannersFromCms(
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                objMaster.UrlBreakdown.ContentDate,
                Session["memberTradeAccountID"] != null,
                dgFramework.IsMobileDevice());

            var unrestrictedBySiteSection = bannerStore
                .result
                .bannerLocationGroups
                .Where(p => p.bannerLocations.banners != null)
                .Select(p => p.bannerLocations)
                .Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds == null))
                .ToList();

            var restrictedToSiteSections = bannerStore
                .result
                .bannerLocationGroups
                .Where(p => p.bannerLocations.banners != null)
                .Select(p => p.bannerLocations)
                .Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds != null))
                .ToList();

            if (siteSectionId != null)
            {
                restrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds.Contains(siteSectionId.Value)).ToList();
                });
            }

            var bannersForThisSiteSection = unrestrictedBySiteSection.Concat(restrictedToSiteSections).ToList();

            var unrestrictedByPage = bannersForThisSiteSection.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var restrictedByPage = bannersForThisSiteSection.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds != null)).ToList();

            restrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds.Contains(objMaster.UrlBreakdown.ContentId)).ToList();
            });

            var availableBanners = unrestrictedByPage.Concat(restrictedByPage).ToList();

            var headerBanners = bannerStore.result.bannerLocationGroups.Where(p => p.name == "Header").Select(p => p.bannerLocations).ToList();

            var headerBannersUnrestrictedBySiteSection = headerBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.siteSectionIds == null)).ToList();

            var headerBannersRestrictedToSiteSections = headerBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.siteSectionIds != null)).ToList();

            if (siteSectionId != null)
            {
                headerBannersRestrictedToSiteSections.ForEach(c => 
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds.Contains(siteSectionId.Value)).ToList();
                });
            }

            var availableHeaderBanners = headerBannersUnrestrictedBySiteSection.Concat(headerBannersRestrictedToSiteSections);

            var headerBannersUnrestrictedByPage = availableHeaderBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var headerBannersRestrictedByPage = availableHeaderBanners.Where(p => p.banners != null && p.banners.Count != 0 && p.banners.All(q => q.restrictions.pageIds != null)).ToList();

            headerBannersRestrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds.Contains(objMaster.UrlBreakdown.ContentId)).ToList();
            });


            var footerBanners = bannerStore.result.bannerLocationGroups.Where(p => p.name == "Footer").Select(p => p.bannerLocations).ToList();

            var footerBannersUnrestrictedBySiteSection = footerBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds == null)).ToList();

            var footerBannersRestrictedToSiteSections = footerBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.siteSectionIds != null)).ToList();

            if (siteSectionId != null)
            {
                footerBannersRestrictedToSiteSections.ForEach(c =>
                {
                    c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.siteSectionIds.Contains(siteSectionId.Value)).ToList();
                });
            }

            var availableFooterBanners = footerBannersUnrestrictedBySiteSection.Concat(footerBannersRestrictedToSiteSections);

            var footerBannersUnrestrictedByPage = availableFooterBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds == null)).ToList();

            var footerBannersRestrictedByPage = availableFooterBanners.Where(p => p.banners != null && p.banners.All(q => q.restrictions.pageIds != null)).ToList();

            footerBannersRestrictedByPage.ForEach(c =>
            {
                c.banners = c.banners == null ? new List<templateBanner>() : c.banners.Where(p => p.restrictions.pageIds.Contains(objMaster.UrlBreakdown.ContentId)).ToList();
            });

            ViewBag.headerBanners = headerBannersUnrestrictedByPage.Concat(headerBannersRestrictedByPage).ToList();

            ViewBag.footerBanners = footerBannersUnrestrictedByPage.Concat(footerBannersRestrictedByPage).ToList();

            ViewBag.banners = availableBanners;
        }

        private bool URLKeywordsRequireCorrecting(string country, string correctString, string currentString)
        {
            if (correctString == null)
            {
                return false;
            }

            if (String.IsNullOrEmpty(correctString))
            {
                correctString = country.ToLowerInvariant();
            }
            else
            {
                correctString = $"{country.ToLowerInvariant()}/{correctString}";
            }

            if (currentString.StartsWith("/"))
            {
                if (currentString.Length > 1)
                {
                    currentString = currentString.Substring(1);
                }
                else
                {
                    currentString = "";
                }
            }

            if (correctString == HttpUtility.UrlDecode(currentString))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task<Tuple<string, NameValueCollection>> FilterIndexingURLKeywordsRequireCorrecting(string country, string intended, string current, byte languageId, byte? productTypeId, BaseViewModel objBase)
        {
            string correction = null;

            var attributeLoader = new cmsAttribute();
            var attributesTask = attributeLoader.loadAttributeFromCms(languageId);

            var combinationsLoader = new cmsFilterIndexAttributeCombination();
            var filterIndexCombinationsTask = combinationsLoader.loadFilterIndexingAttributeCombinationsFromCmsAsync(new PassFilterIndexAttributeCombinationCacheFetch(languageId, productTypeId, objBase.Date, country), objBase);

            await Task.WhenAll(attributesTask, filterIndexCombinationsTask);

            var attributes = await attributesTask;

            var filterIndexCombinations = await filterIndexCombinationsTask;

            attributes.attributeGroups.ForEach(ag =>
            {
                ag.Attributes = attributes.attributeDetails.Where(p => p.AttributeGroupID == ag.AttributeGroupID).ToList();
            });

            foreach (var combination in filterIndexCombinations.Combinations.Where(c => c.Enabled))
            {
                var attributeIds = combination.AttributeSignature.Split(new char[] { '.' }).Select(a => Convert.ToInt16(a));

                foreach (var attributeId in attributeIds)
                {
                    var attribute = attributes.attributeDetails.SingleOrDefault(ad => ad.AttributeID == attributeId);

                    if (attribute != null)
                    {
                        current = current.Replace($"{attribute.FormValue}-", "");
                    }
                }
            }

            var qs = System.Web.HttpUtility.ParseQueryString(string.Empty);

            foreach (var combination in filterIndexCombinations.Combinations.Where(c => c.Enabled == false))
            {
                foreach(var attributeGroup in attributes.attributeGroups.Where(ag => ag.Attributes?.Any() == true))
                {
                    var attributeIds = combination.AttributeSignature.Split(new char[] { '.' }).Select(a => Convert.ToInt16(a));

                    foreach (var attributeId in attributeIds)
                    {
                        if (attributeGroup.Attributes.Select(a => a.AttributeID).Contains(attributeId) == true)
                        {
                            var attribute = attributes.attributeDetails.SingleOrDefault(ad => ad.AttributeID == attributeId);

                            if (attribute != null)
                            {
                                if (current.Contains(attribute.FormValue))
                                {
                                    qs.Add(attributeGroup.FormName, attribute.FormValue);
                                }
                            }
                        }
                    }
                }
            }

            intended = $"{country.ToLowerInvariant()}/{intended.ToLowerInvariant()}";

            var regex = new System.Text.RegularExpressions.Regex(@"([^\s]*\/?)");

            var intendedSegmentsOfInterest = regex.Match(intended);

            var currentSegmentsOfInterest = regex.Match(current);

            if ((current != intended) && currentSegmentsOfInterest.Captures.Count == 1)
            {
                correction = $"{intendedSegmentsOfInterest.Value}{regex.Replace(current, "")}";
            }

            return new Tuple<string,NameValueCollection>(correction, qs);
        }

        // Where is this called from... this is called from above, on every session... so do the local SQL check.
        private async Task tradeAccountCheck(cmsCountryDefinition objCountryData, urlBreakdown urlData, VisitorSelect objVisitorSelect)
        {
            if (objVisitorSelect.memberTradeAccountNumber == null || objVisitorSelect.Email == null)
            {
                Session["memberTradeChecksPassed"] = false;
            }
            else
            {
                var thisCountryTradePriceLists = objCountryData.countryTradePriceLists
                    .Where(p => objCountryData.countryItems.Where(q => q.CountryID == urlData.countryDetails.CountryID)
                                                           .Select(q => q.PriceListID).Contains(p.PriceListID)
                    ).ToList();

                Session["memberTradeChecksPassed"] = false;

                if (thisCountryTradePriceLists.Count > 0)
                {
                    if (objVisitorSelect.memberTradeAccountNumber != null)
                    {
                        if (objVisitorSelect.tradePriceListId != null)
                        {
                            if (thisCountryTradePriceLists.Select(p => p.TradePriceListID).ToList().Contains(objVisitorSelect.tradePriceListId.Value))
                            {
                                // This is what should be being performed EVERYWHERE except for initial registration and checkout entry.
                                var placeOrderPermission = await new VisitorFactory().tradePermissionCheck(objVisitorSelect.memberId, urlData.countryDetails.CountryID);

                                if (placeOrderPermission)
                                {
                                    Session["memberTradeChecksPassed"] = true;

                                    var tradeSignInCookie = new HttpCookie("dg-trade-signin", urlData.countryDetails.URL);

                                    tradeSignInCookie.Expires = DateTime.Now.AddMonths(1);

                                    Response.Cookies.Add(tradeSignInCookie);
                                }
                            }
                            else 
                            {
                                // This user is not permitted to trade order in this market
                                // Are they permitted to trade order in any other market?
                                // If so, set a Session variable, look for it when rendering the site corrector, and
                                // advise them that in order to use the B2B Site they must change site culture?

                                var correctTradePriceList = objCountryData.countryTradePriceLists.SingleOrDefault(tpl => tpl.TradePriceListID == objVisitorSelect.tradePriceListId);

                                var correctSiteCulture = objCountryData.countryItems.SingleOrDefault(ci => ci.PriceListID == correctTradePriceList?.PriceListID);

                                Session["memberTradeCorrectSiteCulture"] = correctSiteCulture;
                            }
                        }
                    }
                }
            }
        }


        public async Task<ActionResult> TrackingJSON(masterDefinition objMaster)
        {
            var objBasket = new basketRoutines();

            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = (Int32?)Session["memberIdFromDatabase"];
            }

            Int64 visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());

            Int64? checkoutId = null;

            if (objMaster.UrlBreakdown.ContentType == dgContentType.landingPage &&
                objMaster.UrlBreakdown.ContentId == (int)dgFixedPage.checkoutConfirmation &&
                dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == true)
            {
                checkoutId = Convert.ToInt64(Session["checkoutId"]);
            }
            
            var objReturn = await objBasket.callGetBrontoJSON(
                visitorDbId,
                memberId,
                objMaster.UrlBreakdown.ContentId,
                checkoutId,
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                (Int32?)Session["memberTradeAccountID"],
                DateTime.Now
            );

            objReturn.objMaster = objMaster;

            return PartialView("~/Views-New/Shared/Components/_trackingJson.cshtml", objReturn);
        }
    }
}