using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding.Binders;

using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
	public class SearchController : ApiController
	{


		#region Controller routines

		[HttpPost]
		public async Task<filteredSearchResult> search(webapiSearchParameters passSearch)
		{

			if (passSearch == null)
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
			}
			else if (passSearch.filterIds == null)
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
			}

			var tradeChecksPassed = false;

			if (HttpContext.Current.Session["memberTradeChecksPassed"] != null)
			{
				tradeChecksPassed = Convert.ToBoolean(HttpContext.Current.Session["memberTradeChecksPassed"]);
			}

			searchGenerate objSearchGenerate = new searchGenerate();
			filteredSearchResult objSearchFilterReturn = await objSearchGenerate.serverFiltering(passSearch, tradeChecksPassed);

			return objSearchFilterReturn;
		}


		[HttpPost]
		public async Task<IHttpActionResult> renderProductPages(webapiRenderProductPages dto)
		{
			if (!dto.productPageIds.Any())
			{
				return Ok(new object[] { });
			}

			Int64 visitorDbId = -1;

			var session = HttpContext.Current.Session;

			if (session["visitorIdFromDatabase"] == null)
			{
			}
			else
			{
				visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
			}

			Int32? memberId = null;
			if (session["memberIdFromDatabase"] == null)
			{
			}
			else
			{
				memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
			}

			var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

			var useDate = Framework.GetUseDate(requestWrapper);
			var useCountry = Framework.GetCountryOverride(requestWrapper);

			if (dto == null)
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
			}

			String basketCountry = dto.basketCountry.Trim();

			cmsCountry objCountry = new cmsCountry();
			cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(useDate);

			if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
			else
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
			}

			countryItem objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

			//  Set the resource file language
			Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objCountryItem.DotNetCulture);
			Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

			//  Get the render details for the PageIDs that are to be shown            
			var productPageToRenderTable = new DataTable();
			productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
			productPageToRenderTable.Columns.Add("Sequence", typeof(int));

			Int32 sequence = 0;
			foreach (Int32 productPageId in dto.productPageIds)
			{
				productPageToRenderTable.Rows.Add(productPageId, sequence++);
			}

			var tradeChecksPassed = false;

			if (HttpContext.Current.Session["memberTradeChecksPassed"] != null)
			{
				tradeChecksPassed = Convert.ToBoolean(HttpContext.Current.Session["memberTradeChecksPassed"]);
			}

			var displayTradePrice = false;

			if (HttpContext.Current.Session["memberDisplayTradePrice"] == null) { }
			else
			{
				displayTradePrice = Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]);
			}

			int? tradeAccountId = null;

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}

			var objSearchGenerate = new searchGenerate();

			var productPages = await objSearchGenerate.getRenderForSearchResults(tradeAccountId, tradeChecksPassed, displayTradePrice, productPageToRenderTable, useDate, objCountryItem, $"{useDate:O}", useCountry);

			var db = new databaseDataContext();

			var pricing = await db.ProductPage_Price_SelectAsync(new Models.DTOs.ProductPage_Price_SelectDTO()
			{
				countryCheckMatched = Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
				countryId = dto.countryIdOverride ?? objCountryItem.CountryID,
				languageId = objCountryItem.LanguageID,
				isLifestyle = HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
				objVisitor = new VisitorSelect()
				{
					visitorDbId = visitorDbId,
					memberId = memberId,
					memberTradeAccountID = tradeAccountId
				},
				displayTradePrice = displayTradePrice,
				objMaster = new NewMasterDefinition()
				{
					UrlBreakdown = new urlBreakdown()
					{
						countryDetails = objCountryItem,
						ContentDate = useDate
					}
				},
				productPageIds = dto.productPageIds
			});

			var results = JArray.FromObject(JObject.FromObject(productPages.json).SelectToken("productPages")).Children().ToList();

			//foreach (var pp in results)
			//{
			//    var productPageId = JObject.FromObject(c).SelectToken("productPageId").Value<int>();

			//    var productPagePrice = JArray.FromObject(JObject.FromObject(pricing.json).SelectToken("productPages")).Where(x => JObject.FromObject(x).SelectToken("productPageId").Value<int>() == productPageId).SingleOrDefault();

			//    pp["productPagePrice"] = productPagePrice;
			//}

			results.ForEach(c =>
			{
				var productPageId = JObject.FromObject(c).SelectToken("productPageId").Value<int>();

				if (pricing != null)
				{
					if (pricing.json != null)
					{
						var productPagePrice = JArray.FromObject(JObject.FromObject(pricing.json).SelectToken("productPages")).Where(x =>
							JObject.FromObject(x).SelectToken("productPageId").Value<int>() == productPageId
						).SingleOrDefault();

						if (productPagePrice != null)
						{
							c["productPagePrice"] = JObject.FromObject(productPagePrice).SelectToken("productPage.productPagePrice");
						}
					}
				}
			});

			return Ok(results);
		}

		[HttpPost]
		public async Task<object> getPredictiveSearch(webapiPredictiveSearch objSearch)
		{
			var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

			var productTypeDropDown = await new cmsSearchProductType().loadSearchProductTypeDropDownFromCms(
				objSearch.countryId,
				objSearch.languageId,
				useDate);

			var productTypeId = productTypeDropDown.ProductTypes.SingleOrDefault(pt => pt.FormValue == objSearch.productType)?.ProductTypeID;

			if (objSearch == null)
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidParameters);
			}
			else if (objSearch.searchTerm == null)
			{
				return new predictiveSearchResult();
			}

			objSearch.searchTerm = objSearch.searchTerm.Trim();

			if (objSearch.searchTerm == "")
			{
				return new predictiveSearchResult();
			}

			var basketCountry = objSearch.siteCountry.Trim();

			var objCountry = new cmsCountry();

			var objCountryData = await objCountry.countryLanguageSelect(useDate);

			if (dgFramework.doesCountryExistFromText(basketCountry, objCountryData)) { }
			else
			{
				dgFramework.raiseErrorNotHandled(webapiUnhandledError.InvalidCountry);
			}

			int? tradeAccountId = null;

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}

			var objCountryItem = dgFramework.getCountryFromText(basketCountry, objCountryData);

			var objSearchGenerate = new searchGenerate();

			var hintsTask = objSearchGenerate.getPredictiveSearchHints(objSearch.searchTerm, objCountryItem.LanguageID, objCountryItem.CountryID, useDate, productTypeId, tradeAccountId);
			var resultsTask = objSearchGenerate.getPredictiveSearch(objSearch.searchTerm, objCountryItem.LanguageID, objCountryItem.CountryID);

			await Task.WhenAll(hintsTask, resultsTask);

			var objReturn = new passPredictiveSearch
			{
				predictiveSearchHints = await hintsTask,
				predictiveSearchResults = await resultsTask
			};

			foreach (var pt in objReturn.predictiveSearchHints.productTypes)
			{
				pt.URLKeywords = dgFramework.buildUrl(objCountryItem, null, dgContentType.landingPage, (int)dgFixedPage.serverSearch, HttpContext.Current.Request.QueryString["dateOverride"], HttpContext.Current.Request.QueryString["countryOverride"], $"search-term={objSearch.searchTerm}&product-type={pt.FormValue}");
			}
			foreach (var c in objReturn.predictiveSearchHints.collections)
			{
				c.URLKeywords = dgFramework.buildUrl(objCountryItem, c.URLKeywords, dgContentType.collectionPage, c.CollectionID, HttpContext.Current.Request.QueryString["dateOverride"], HttpContext.Current.Request.QueryString["countryOverride"]);
			}
			foreach (var d in objReturn.predictiveSearchHints.designs)
			{
				d.URLKeywords = dgFramework.buildUrl(objCountryItem, d.URLKeywords, dgContentType.productPage, d.ProductPageID, HttpContext.Current.Request.QueryString["dateOverride"], HttpContext.Current.Request.QueryString["countryOverride"]);
			}
			foreach (var p in objReturn.predictiveSearchHints.products)
			{
				p.URL = dgFramework.buildUrl(objCountryItem, p.URL, dgContentType.productPage, p.ProductPageID, HttpContext.Current.Request.QueryString["dateOverride"], HttpContext.Current.Request.QueryString["countryOverride"]);
			}
			foreach (var p in objReturn.predictiveSearchHints.brands)
			{
				p.URLKeywords = dgFramework.buildUrl(objCountryItem, p.URLKeywords, dgContentType.landingPage, p.PageID, HttpContext.Current.Request.QueryString["dateOverride"], HttpContext.Current.Request.QueryString["countryOverride"]);
			}

			return objReturn;
		}

		#endregion

		[HttpPost]
		public async Task<object> getQuickLookOverlay(webapiQuickLook passQuickLook)
		{
			Int64 visitorDbId = -1;

			var session = HttpContext.Current.Session;

			if (session["visitorIdFromDatabase"] == null)
			{
			}
			else
			{
				visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
			}

			Int32? memberId = null;
			if (session["memberIdFromDatabase"] == null)
			{
			}
			else
			{
				memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
			}

			//  Determine the date to obtain the data from the cms
			DateTime useDate = DateTime.Now;
			useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
			if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
			else
			{
				//  Should the "use date" be overriden?
				if (passQuickLook.Date == null)
				{
					useDate = DateTime.Now;
				}
				else
				{
					String overrideDate = passQuickLook.Date;
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
							useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
						}
					}
				}
			}

			cmsProductPage objCms = new cmsProductPage();

			quickLookReturn objReturn = await objCms.loadQuickLookFromCms(visitorDbId, memberId, passQuickLook.ProductPageID, passQuickLook.CountryID, passQuickLook.LanguageID, useDate, (int?)session["memberTradeAccountID"]);

			return objReturn;
		}
	}
}
