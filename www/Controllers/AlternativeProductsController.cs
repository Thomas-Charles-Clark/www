using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.Stockists;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.Controllers
{
    public class AlternativeProductsController : Controller
    {
        // GET: AlternativeProducts
        public async Task<ActionResult> Index(string Code)
        {
            VisitorFactory objVisitorFactory = new VisitorFactory();

            VisitorSelect objVisitorSelect = new VisitorSelect();

            #region DETERMINE           ContentDate

            //  Determine the date to use when obtaining data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);

            // Only if we're not in Production mode can we travel through time...
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = Request.QueryString["dateOverride"].ToString();
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

			var cfVerifiedBot = Request.Headers["cf-verified-bot"];
			var cfBotScore = Request.Headers["cf-bot-score"];
			var ja3fingerprints = Request.Headers["cf-ja3-hash"];
			var ja4fingerprints = Request.Headers["cf-ja4-hash"];

			var verifiedBot = false;
			var botScore = 100;

			if (Boolean.TryParse(cfVerifiedBot, out bool v))
			{
				verifiedBot = v;
			}

			if (Int32.TryParse(cfBotScore, out int bs))
			{
				botScore = bs;
			}

            if (verifiedBot == true)
            {
                dgFramework.setVerifiedBot(ref objVisitorSelect);
			}
            else
			{
				//  Get the session identifier
				if (Session["visitorIdFromDatabase"] == null)
				{
					// Looks for VisitorID using GUID in cookie

					String cookieGuid = dgFramework.getIdentificationFromCookie();

					objVisitorSelect = await dgFramework.identifySession(objVisitorSelect, objVisitorFactory, cookieGuid, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

					// The beneath four can be null...
					Session["memberName"] = objVisitorSelect.FullName;
					Session["memberEmail"] = objVisitorSelect.Email;
					Session["visitorCulture"] = objVisitorSelect.Culture;
					Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
					Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
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

					if (Session["visitorCulture"] == null) { }
					else
					{
						objVisitorSelect.Culture = Session["visitorCulture"].ToString();
					}
				}
			}

            // If we have a VisitorID to use, this saves the "Culture" column on [mbr].[Visitor]
            // If not, this creates a new Visitor storing UserAgent, IP and Culture, saves the returned GUID to cookie and gives us the VisitorID
            if (objVisitorSelect.visitorDbId == -1)
            {
                identificationDefinition identDef = await dgFramework.generateNewVisitorId(objVisitorFactory, dgFramework.getUserAgent(), dgFramework.getIPAddress(), null, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

                objVisitorSelect.visitorDbId = identDef.visitorDbId;

                dgFramework.saveIdentificationToCookie(identDef.userGuid);

                Session["memberIdFromDatabase"] = objVisitorSelect.memberId;
                Session["visitorIdFromDatabase"] = objVisitorSelect.visitorDbId.ToString();
                Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
                Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
                Session["memberDisplayTradePrice"] = objVisitorSelect.memberDisplayTradePrice;
                Session["visitorCreatedDT"] = objVisitorSelect.VisitorCreatedDT;

            }

            string cfIPCountryCode = Request.Headers["CF-IPCountry"];

            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                if (Request.QueryString["countryOverride"] == null)
                {
                    if (cfIPCountryCode == null)
                    {
                        cfIPCountryCode = "GB";
                    }
                }
            }

            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

            var countryDetails = new countryItem();
            var countryId = (short)81; // default to UK...
            var languageId = (byte)1;

            if (dgFramework.doesCountryExistFromCode(cfIPCountryCode, objCountryData))
            {
                countryDetails = dgFramework.getCountryFromCode(cfIPCountryCode, objCountryData);
                countryId = countryDetails.CountryID;
                languageId = countryDetails.LanguageID;
            }
            
            // bollocks, it does need to run through page select...
            masterDefinition objMaster = new masterDefinition();
            
            cmsLandingPage objLandingPage = new cmsLandingPage();
            cmsLandingPageDefinition objPageData = await objLandingPage.loadLandingPageFromCms(
                                                                                    objVisitorSelect.memberId,
                                                                                    1515,
                                                                                    languageId,
                                                                                    countryId,
                                                                                    useDate,
                                                                                    objVisitorSelect.memberId != null,
                                                                                    objVisitorSelect.memberTradeAccountID != null,
                                                                                    Convert.ToBoolean(Session["countryCheckMatched"]),
                                                                                    objVisitorSelect.memberDisplayTradePrice,
                                                                                    objVisitorSelect.memberTradeAccountID);

            basketRoutines objBasket = new basketRoutines();

            var basketTotalData = await objBasket.loadBasketTotalQuantity(objVisitorSelect.visitorDbId, objVisitorSelect.memberId, countryId);

            objMaster.TitleTag = objPageData.PageDetails.TitleTag;
            objMaster.MetaDescription = objPageData.PageDetails.MetaDescription;
            
            objMaster.ItemsInBasket = basketTotalData.TotalQuantity;

            objMaster.UrlBreakdown = new urlBreakdown()
            {
                ContentDate = useDate,
                ContentId = 1515,
                ContentType = dgContentType.landingPage,
                contentTypeAndIdNotFound = false,
                CountryAwareness = new countryAwarenessCheckReturn()
                {
                    Culture = countryDetails.DotNetCulture,
                    Match = true,
                    objCountryItem = countryDetails
                },
                countryDetails = countryDetails,
                GeoIPCulture = null,
                SemanticString = "alternative-products" 
            };

            objMaster.googleHrefs = objPageData.pageGoogleHrefs;
            
            var Model = new AlternativeProductsViewModel();

            var db = new databaseDataContext();

            var results = db.AlternativeProducts_GenerateMultiple(
                Code, 
                useDate, 
                languageId, 
                countryId, 
                objVisitorSelect.memberTradeAccountID != null,
                objVisitorSelect.memberTradeAccountID, 
                objVisitorSelect.memberDisplayTradePrice);

            Model.masterDefinition = objMaster;
            Model.LandingPage = objPageData;
            Model.SourcePods = results.GetResult<searchResultRenderCore>().ToList();
            Model.SourcePodImages = results.GetResult<searchResultRenderImage>().ToList();
            Model.SourceProductPageAttributes = results.GetResult<ProductPageAttribute>().ToList();
            Model.AlternativePods = results.GetResult<searchResultRenderCore>().ToList();
            Model.AlternativePodImages = results.GetResult<searchResultRenderImage>().ToList();
            Model.AlternativeProductPageAttributes = results.GetResult<ProductPageAttribute>().ToList();

            var stockStatus = new cmsStockStatus();
            var stockStatuses = await stockStatus.loadStockStatusesFromCms(languageId, countryId, useDate);

            Model.SourcePods.ForEach(c => 
            {
                c.StockStatus = stockStatuses.StockStatuses.Where(p => p.StockStatusID == c.StockStatusID).SingleOrDefault()?.StockStatus;
            });
            
            Model.AlternativePods.ForEach(c => 
            {
                c.StockStatus = stockStatuses.StockStatuses.Where(p => p.StockStatusID == c.StockStatusID).SingleOrDefault()?.StockStatus;
            });

            return View(dgFramework.GetPreferredDeviceView("~/Views/AlternativeProducts/AlternativeProducts{0}.cshtml"), Model);
        }
    }

    public class AlternativeProductsViewModel
    {
        public masterDefinition masterDefinition { get; set; }
        public cmsLandingPageDefinition LandingPage { get; set; }
        public List<searchResultRenderCore> SourcePods { get; set; }
        public List<searchResultRenderImage> SourcePodImages { get; set; }
        public List<ProductPageAttribute> SourceProductPageAttributes { get; set; }
        public List<searchResultRenderCore> AlternativePods { get; set; }
        public List<searchResultRenderImage> AlternativePodImages { get; set; }
        public List<ProductPageAttribute> AlternativeProductPageAttributes { get; set; }

        public AlternativeProductsViewModel()
        {
            SourcePods = new List<searchResultRenderCore>();
            SourcePodImages = new List<searchResultRenderImage>();
            SourceProductPageAttributes = new List<ProductPageAttribute>();
            AlternativePods = new List<searchResultRenderCore>();
            AlternativePodImages = new List<searchResultRenderImage>();
            AlternativeProductPageAttributes = new List<ProductPageAttribute>();
        }
    }
}