using DevExpress.Web.Mvc.Internal;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using MertexerV2.Models.Accounts;

using MertexerV2SQL.Accounts;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NLog;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ClientError;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.DTOs;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.JsonLD;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Models.SearchRender;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;
using wwwDesignersGuild.ViewModels.Trade;

namespace wwwDesignersGuild.Controllers
{
	public class GeneralAPIController : ApiController
	{
		[HttpPost]
		public async Task<IHttpActionResult> addressSearch([FromBody] PassPCAPredictFind objSearch)
		{
			var loginCheck = dgFramework.ensureMemberLoginWebApi(HttpContext.Current.Session, false);

			if (loginCheck.loginStatus != dgLoginStatus.currentLogin)
			{
				return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidLogin));
			}

			try
			{
				if (objSearch.mode == "extant")
				{
					if (loginCheck.tradeAccountId == null ||
						loginCheck.tradeDataSourceJson == null)
					{
						return Ok(dgFramework.raiseErrorToHandle(webapiDataError.checkoutTradeInvalidPermission));
					}

					var tradeAccountNumber = JsonConvert.DeserializeObject<MertexerV2.Models.DataSource.TradeAccountDataSourceInfo>(loginCheck.tradeDataSourceJson)
						.EnquiryTypes
						.Where(p => p.EnquiryType == MertexerV2.Models.DataSource.EnquiryType.Address)
						.Select(p => p.DataSources.First().CustomerReference)
						.Single();

					var countries = new cmsCountry();
					var countryResults = await countries.countryLanguageSelect(DateTime.Now);

					var mertexResults = new List<MertexPCALookupResult>();

					if (objSearch.text?.Length > 2)
					{
						mertexResults = AccountFunctionsUniSQL.AddressBookSearch(loginCheck.tradeDataSourceJson, objSearch);

						mertexResults.ForEach(c =>
						{
							var item = countryResults.countryItems.FirstOrDefault(p => p.CountryCode == c.Country);
							if (item != null)
							{
								c.CountryID = item.CountryID;
								c.Country = item.CountryCode;
							}
						});

						mertexResults = mertexResults.GroupBy(p => p.MD5Hash).Select(p => p.OrderBy(q => q.DeliveryReference).Last()).ToList();
					}

					var removeMarkedAsDeleted = new List<MertexPCALookupResult>();

					var db = new databaseDataContext();

					var tradeAccountAddressCleanseRecords = db.tradeAccountAddressCleanseList(tradeAccountNumber);

					mertexResults.ForEach(c =>
					{
						var extantCleanse = tradeAccountAddressCleanseRecords.Where(p =>
							p.TradeAccountCode == c.TradeAccountCode &&
							p.DeliveryReference == c.DeliveryReference).SingleOrDefault();

						if (extantCleanse != null)
						{
							if (extantCleanse.DeletedDT != null)
								removeMarkedAsDeleted.Add(c);
						}
						else
						{
							var a = new TradeAccountAddressCleanse()
							{
								TradeAccountCode = c.TradeAccountCode,
								DeliveryReference = c.DeliveryReference,
								SourceAddressHash = c.MD5Hash,
								SourceAddress = JsonConvert.SerializeObject(c),
								Status = null,
								CreatedDT = DateTime.Now,
								LookupDT = null,
								DeletedDT = null
							};

							db.insertTradeAccountAddressClease(a);
						}
					});

					mertexResults = mertexResults.Except(removeMarkedAsDeleted).ToList();

					return Ok(new { mertexResults = mertexResults.OrderByDescending(p => p.DeliveryReferenceID).Take(50).ToList(), results = new object[] { } });
				}
				else if (objSearch.mode == "member")
				{
					var memberAddressResults = await new VisitorFactory().getMemberAddresses(loginCheck.visitorId.Value, null, Convert.ToByte(objSearch.language), null, DateTime.Now);

					if (!String.IsNullOrEmpty(objSearch.text))
					{
						var searchText = objSearch.text.ToLowerInvariant().Trim();

						memberAddressResults.Addresses = memberAddressResults.Addresses.Where(p =>
						{
							return p.FullName?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.CompanyName?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.AddressLine1?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.AddressLine2?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.Town?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.County?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.PostCode?.ToLowerInvariant().StartsWith(searchText) == true ||
									p.CountryCode?.ToLowerInvariant() == searchText;
						}).ToList();
					}

					return Ok(new
					{
						mertexResults = memberAddressResults.Addresses.Select(a => new MertexPCALookupResult()
						{
							DeliveryReference = $"{a.MemberAddressID}",
							DeliveryReferenceID = -1,
							TradeAccountCode = null,
							City = a.Town,
							Company = a.CompanyName,
							Country = a.CountryCode,
							CountryID = a.CountryID,
							Email = null,
							Line1 = a.AddressLine1,
							Line2 = a.AddressLine2,
							Name = a.FullName,
							Phone = a.PhoneNumber,
							PostalCode = a.PostCode,
							Province = a.County
						}),
						results = new object[] { }
					});
				}
				else
				{
					if (objSearch.id == null)
					{
						var client = new PCAFind.PostcodeAnywhere_SoapClient();

						var results = new PCAFind.Capture_Interactive_Find_v1_00_ArrayOfResults();

						if (!String.IsNullOrEmpty(objSearch.text))
						{
							results = client.Capture_Interactive_Find_v1_00(
								Key: ConfigurationManager.AppSettings["PCA-API-Website-UK"],
								Text: objSearch.text,
								Container: objSearch.container,
								Origin: objSearch.origin,
								Countries: objSearch.countries,
								Limit: 12,
								Language: "en-GB"
							);
						}

						return Ok(new { mertexResults = new object[] { }, results });
					}
					else
					{
						var client = new PCARetrieve.PostcodeAnywhere_SoapClient();

						var results = client.Capture_Interactive_Retrieve_v1_00(
							Key: ConfigurationManager.AppSettings["PCA-API-Website-UK"],
							Id: objSearch.id,
							Field1Format: null,
							Field2Format: null,
							Field3Format: null,
							Field4Format: null,
							Field5Format: null,
							Field6Format: null,
							Field7Format: null,
							Field8Format: null,
							Field9Format: null,
							Field10Format: null,
							Field11Format: null,
							Field12Format: null,
							Field13Format: null,
							Field14Format: null,
							Field15Format: null,
							Field16Format: null,
							Field17Format: null,
							Field18Format: null,
							Field19Format: null,
							Field20Format: null
						);

						return Ok(new { mertexResults = new object[] { }, results });
					}
				}
			}
			catch (Exception ex)
			{
				var exceptionResult = new HttpResponseMessage(HttpStatusCode.InternalServerError)
				{
					Content = new StringContent("The Address Lookup call failed: " + ex.ToString())
				};

				return ResponseMessage(exceptionResult);
			}
		}

		[HttpPost]
		public async Task<IHttpActionResult> FetchProductPagePrices([FromBody] PassProductPageIDs dto)
		{
			dto.productPageIds = dto.productPageIds.Distinct().ToList();

			Int64 visitorId = -1;
			if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
			{
			}
			else
			{
				visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
			}

			Int32? memberId = null;
			if (HttpContext.Current.Session["memberIdFromDatabase"] == null)
			{
			}
			else
			{
				memberId = Convert.ToInt32(HttpContext.Current.Session["memberIdFromDatabase"].ToString());
			}

			bool displayTradePrice = false;
			if (HttpContext.Current.Session["memberDisplayTradePrice"] == null)
			{

			}
			else
			{
				displayTradePrice = Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"].ToString());
			}

			var memberTradeAccountId = (int?)HttpContext.Current.Session["memberTradeAccountID"];

			var db = new databaseDataContext();

			var visitor = new VisitorSelect()
			{
				visitorDbId = visitorId,
				memberId = memberId,
				memberTradeAccountID = memberTradeAccountId
			};

			var result = await db.ProductPage_Price_SelectAsync(new ProductPage_Price_SelectDTO()
			{
				productPageIds = dto.productPageIds,
				objVisitor = visitor,
				objMaster = dto.objMaster,
				displayTradePrice = displayTradePrice
			});

			return Ok(result.json);
		}


		public async Task<IHttpActionResult> FetchTechInfo([FromBody] PassProductPageID objProductPage)
		{
			var ppcms = new cmsProductPage();

			var result = await ppcms.loadTechInfoFromCms(objProductPage.productPageId, objProductPage.objMaster.UrlBreakdown.countryDetails.CountryID,
				objProductPage.objMaster.UrlBreakdown.countryDetails.LanguageID,
				objProductPage.objMaster.UrlBreakdown.ContentDate,
				Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]));

			return Ok(result.json);
		}

		public IHttpActionResult FetchRecentlyViewed([FromBody] PassProductPageID objProductPage)
		{
			var db = new Models.Database.databaseDataContext();

			Int64 visitorId = -1;
			if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
			{
			}
			else
			{
				visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
			}

			var result = db.FetchRecentlyViewed(
				objProductPage,
				visitorId,
				((int?)HttpContext.Current.Session["memberTradeAccountID"]).HasValue);

			return Ok(result.json);
		}

		public IHttpActionResult JsonLD([FromBody] PassJsonObjs jsonObjs)
		{
			var result = new Helpers.JsonLD(jsonObjs.type, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(jsonObjs.json)), jsonObjs.description, jsonObjs.currency);

			var jsonResult = result.ToHtmlString();

			var unfurling = result.Unfurling();

			return Ok(new { jsonLd = jsonResult, unfurling });
		}

		public async Task<IHttpActionResult> FetchModularOptions([FromBody] PassProductPageID objProductPage)
		{
			var ppcms = new cmsProductPage();

			var result = await ppcms.loadModularInfoFromCms(objProductPage.productPageId, objProductPage.objMaster.UrlBreakdown.countryDetails.CountryID,
				objProductPage.objMaster.UrlBreakdown.countryDetails.LanguageID,
				objProductPage.objMaster.UrlBreakdown.ContentDate,
				Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]));

			return Ok(result.json);
		}

		[HttpPost]
		public IHttpActionResult LogClientError([FromBody] PassClientError objError)
		{
			Int64 visitorId = -1;
			if (HttpContext.Current.Session["visitorIdFromDatabase"] == null)
			{
			}
			else
			{
				visitorId = Convert.ToInt64(HttpContext.Current.Session["visitorIdFromDatabase"].ToString());
			}

			if (objError?.message?.StartsWith("QuotaExceededError") == true)
			{
				return Ok(new { });
			}

			new errorDB().saveToErrorLog(
				visitorId,
				"Client-Side Error",
				objError?.message,
				objError?.source,
				objError?.message,
				$"Line No. {objError?.lineno}",
				$"Column No. {objError?.colno}",
				null,
				null,
				objError?.url,
				dgFramework.getUserAgent(),
				dgFramework.getIPAddress());

			return Ok(new { });
		}

		[HttpPost]
		public IHttpActionResult FetchResources([FromBody] PassResourceFetch objFetch)
		{
			var resourceDefinition = new cmsResource().loadResourceFromCms(
				objFetch.languageId,
				objFetch.countryId,
				objFetch.useDate);

			var result = new List<ResourceResult>();

			foreach (var r in objFetch.resources)
			{
				result.Add(new ResourceResult()
				{
					Name = r,
					Value = resourceDefinition.Result.resourceSelects
													 .Where(p => p.Name == r)
													 .Select(p => p.Value)
													 .SingleOrDefault()
				});
			}

			return Ok(result);
		}

		[HttpPost]
		public async Task<IHttpActionResult> SearchRender([FromBody] PassSearchRender dto)
		{
			var db = new databaseDataContext();

			var objSearchGenerate = new searchGenerate();

			var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

			var useDate = Framework.GetUseDate(requestWrapper);

			var countryOverride = Framework.GetCountryOverride(requestWrapper);

			var isLifestyle = HttpContext.Current.Request.QueryString.AllKeys.FirstOrDefault(k => k == "lifestyle") != null;

			var countryId = dto.@base.MasterDefinition.UrlBreakdown.countryDetails.CountryID;

			var languageId = dto.@base.MasterDefinition.UrlBreakdown.countryDetails.LanguageID;

			if (!String.IsNullOrEmpty(countryOverride))
			{
				var objCountry = new cmsCountry();
				var objCountryData = await objCountry.countryLanguageSelect(useDate);

				var countryDetails = new countryItem();

				if (dgFramework.doesCountryExistFromCode(countryOverride, objCountryData))
				{
					countryDetails = dgFramework.getCountryFromCode(countryOverride, objCountryData);
					countryId = countryDetails.CountryID;
					languageId = countryDetails.LanguageID;
				}
			}

			var productPageToRenderTable = new DataTable();

			productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
			productPageToRenderTable.Columns.Add("Sequence", typeof(int));

			int sequence = 0;

			foreach (int productPageId in dto.productPageIds)
			{
				productPageToRenderTable.Rows.Add(productPageId, sequence++);
			}

			var productPages = objSearchGenerate.getRenderForSearchResultsAsync(
				(int?)HttpContext.Current.Session["memberTradeAccountId"],
				((int?)HttpContext.Current.Session["memberTradeAccountId"]).HasValue
					&& Convert.ToBoolean(HttpContext.Current.Session["tradeOrderingAllowed"])
					&& Convert.ToBoolean(HttpContext.Current.Session["memberTradeChecksPassed"]),
				 Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
				productPageToRenderTable,
				useDate,
				dto.@base.MasterDefinition.UrlBreakdown.countryDetails,
				$"{useDate:O}",
				countryOverride);

			var visitor = new VisitorSelect()
			{
				visitorDbId = Int64.Parse(HttpContext.Current.Session["visitorIdFromDatabase"].ToString()),
				memberId = (int?)HttpContext.Current.Session["memberIdFromDatabase"],
				memberTradeAccountID = (int?)HttpContext.Current.Session["memberTradeAccountId"]
			};

			var objPrices = await db.ProductPage_Price_SelectAsync(new ProductPage_Price_SelectDTO()
			{
				productPageIds = dto.productPageIds,
				objVisitor = visitor,
				objMaster = dto.@base.MasterDefinition,
				isLifestyle = isLifestyle,
				countryCheckMatched = dto.@base.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
				displayTradePrice = visitor.memberDisplayTradePrice,
				countryId = dto.@base.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
				languageId = dto.@base.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
				useDate = useDate,
				pageId = null
			});

			var serializer = new JsonSerializer();

			serializer.ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();

			var prices = JObject.FromObject(objPrices.json)
				.SelectToken("productPages")
				.Children()
				.Select(p => p.SelectToken("productPage"))
				.Select(p => new
				{
					productPageId = p.SelectToken("productPageId").Value<int>(),
					price = p.SelectToken("productPagePrice").ToObject<ProductPagePodPrice>(serializer)
				});

			var results = dto.productPageIds
				.Select(p => new ProductPageFilterablePod() { ProductPageID = p })
				.ToList();

			for (var i = 0; i < results.Count; i++)
			{
				var productPageId = results[i].ProductPageID;

				var renderedResult = JObject.FromObject(productPages.json)
											.SelectToken("productPages")
											.Children()
											.Where(p => p.SelectToken("productPageId").Value<int>() == productPageId)
											.SingleOrDefault()
											?.ToObject<ProductPageFilterablePod>();

				if (renderedResult != null)
				{
					renderedResult.ProductPagePrice = prices.Where(p => p.productPageId == productPageId).Select(p => p.price).SingleOrDefault();

					results[i] = renderedResult;
				}
			}

			return Ok(results);
		}

		public async Task<IHttpActionResult> FetchLanguages([FromBody] PassCountryID objCountryId)
		{
			var country = new cmsCountry();

			var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

			var useDate = Framework.GetUseDate(requestWrapper);

			var languages = await country.loadCompleteLanguageList(objCountryId.countryId, useDate);

			return Ok(languages.languages);
		}

		[HttpPost]
		public async Task<IHttpActionResult> CoverageCalculator([FromBody] PassCoverageParameters objFetch)
		{
			var db = new visitorDb();

			CoverageResponse result = null;

			if (objFetch.Captcha == null)
			{
				result = db.requirementsCalculator(objFetch);
			}
			else
			{
				if (ReCaptchaVerifier.Validate2022(objFetch.Captcha) == "true")
				{
					result = db.requirementsCalculator(objFetch);
				}
				else
				{
					return Ok(dgFramework.raiseErrorToHandle(webapiDataError.InvalidCaptcha));
				}
			}

			return Ok(result);
		}

		[HttpPost]
		public async Task<IHttpActionResult> CountryCorrectorDismissed()
		{
			HttpContext.Current.Session["countryCorrectorDismissed"] = true;

			return Ok(new { });
		}

		[HttpPost]
		public async Task<IHttpActionResult> TradeLoginPromptDismissed()
		{
			HttpContext.Current.Session["tradeLoginPromptDismissed"] = true;

			return Ok(new { });
		}


		[HttpPost]
		public async Task<IHttpActionResult> FetchProductPageGridDefinition([FromBody] PassProductPageGridDefinitionFetch dto)
		{
			var session = HttpContext.Current.Session;

			int? memberId = null;

			if (session["memberIdFromDatabase"] == null)
			{

			}
			else
			{
				memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
			}

			int? tradeAccountId = null;

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}
			var requestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

			var useDate = Framework.GetUseDate(requestWrapper);

			var cacheProvider = new ProductPageGridPageSectionCacheProvider();

			var countryCheckMatched = Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]);

			var displayTradePrice = Convert.ToBoolean(session["memberDisplayTradePrice"]);

			var tradeAccountID = (int?)HttpContext.Current.Session["memberTradeAccountID"];

			var rawUrl = HttpContext.Current.Request.RawUrl;

			var result = cacheProvider.GetOrCreate(
				pageSectionId: dto.pageSectionId,
				languageId: dto.languageId,
				countryId: dto.countryId,
				tradeAccountId: tradeAccountId,
				countryMatch: countryCheckMatched,
				useDate: useDate, 
				shouldLock: false,
				factory: () => {

					var creationResult = Task.Run(() => {

						var Definition = Task.Run(() => new cmsLandingPage().loadLandingPageFromCms(
							memberId: memberId,
							pageId: dto.MasterDefinition.UrlBreakdown.ContentId,
							languageId: dto.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
							countryId: dto.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
							useDate: dto.MasterDefinition.UrlBreakdown.ContentDate,
							isMember: memberId.HasValue,
							isTrade: dto.MasterDefinition.TradeMember,
							countryMatch: countryCheckMatched,
							displayTradePrice: displayTradePrice,
							tradeAccountId: tradeAccountId,
							productPageGridPage: dto.page,
							productPageGridPageSize: dto.pageSize)).Result;

						var Section = Task.Run(() => ProductPageGridPageSection.CreateAsync(
							originalUrlKeywords: dto.MasterDefinition.UrlBreakdown.SemanticString,
							productPage: Definition.productPageGridSections.SingleOrDefault(p => p.PageSectionID == dto.pageSectionId),
							pagingEnabled: true,
							filterIndexingEnabled: Definition.PageDetails.EnableFilterIndexing,
							isShop: Definition.PageDetails.IsShop,
							isLifestyleGallery: false,
							tradeAccountID,
							rawUrl,
							model: dto.@base
						)).Result;

						return Section;

					}).Result;

					return creationResult;
				});

			return Ok(result);
		}
	}
}