using Braintree;

using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Wordprocessing;

using iText.Html2pdf.Attach.Impl.Layout.Form.Renderer;

using MertexerV2.Models;
using MertexerV2.SQLFunctions;

using Microsoft.Ajax.Utilities;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using U2.Data.ClientTypes;

using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.ModularFurniture;
using wwwDesignersGuild.Models.OrderForm;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

using static Antlr.Runtime.Tree.TreeWizard;

namespace wwwDesignersGuild.Controllers
{
    public class ProductPageAPIController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> fetchModuleDetail([FromBody] PassModuleOption module)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            var objProductPage = new cmsOrderForm();

            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;

            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter, $"visitorIdFromDatabase was null!");
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

            int? tradeAccountId = null;

            if (session["memberTradeChecksPassed"] != null)
            {
                if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
                {
                    tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
                }
            }

            var furnitureOrderForm = await objProductPage.loadOrderFormFromCms(
                    visitorDbId,
                    memberId,
                    module.ProductPageID,
                    module.LanguageID,
                    module.CountryID,
                    tradeAccountId,
                    Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                    HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
                    Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                    useDate);

            string render = null;

            if (module.FabricProductPageID.HasValue)
            {
                var fabricOrderForm = await objProductPage.loadOrderFormFromCms(
                        visitorDbId,
                        memberId,
                        module.FabricProductPageID.Value,
                        module.LanguageID,
                        module.CountryID,
                        tradeAccountId,
                        Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                        HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
                        Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                        useDate);


                var objBasket = new basketRoutines();

                var basket = await objBasket.loadBasketDetailQuick(
                    visitorDbId,
                    memberId,
                    module.LanguageID,
                    module.CountryID,
                    useDate,
                    tradeAccountId,
                    tradeAccountId.HasValue,
                    Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]));

                var orderFormItemViewModel = new OrderFormItemViewModel(
                    JObject.FromObject(fabricOrderForm.orderForm).SelectToken("products")?.FirstOrDefault(),
                    module.MasterDefinition,
                    new BaseViewModel()
                    {
                        Basket = basket,
                        MasterDefinition = module.MasterDefinition
                    }
                );

                render = dgFramework.RenderViewToString("NewSection", "~/Views-New/ProductPages/Shared/_OrderFormItem.cshtml", orderFormItemViewModel);
            }

            return Ok(new {
                furnitureOrderForm,
                render
            });
        }

		[HttpPost]
		public async Task<IHttpActionResult> FetchProductPageGridPortion([FromBody]ProductPageGrid_PagingDTO dto)
		{
			var objAttribute = new cmsAttribute();

			var objAttributeLoad = objAttribute.loadAttributeFromCms(dto.LanguageID);

			// Thanks https://stackoverflow.com/questions/32119507/multiple-awaits-vs-task-waitall-equivalent#answer-32119616

			await Task.WhenAll(objAttributeLoad); //(, ...);

			var attributeData = await objAttributeLoad;

			var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

			var useCountry = Framework.GetCountryOverride(new HttpRequestWrapper(HttpContext.Current.Request));

			var session = HttpContext.Current.Session;

			Int32? memberId = null;

			if (session["memberIdFromDatabase"] == null)
			{

			}
			else
			{
				memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
			}

			int? tradeAccountId = null;

			if (session["memberTradeChecksPassed"] != null)
			{
				if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
				{
					tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"].ToString());
				}
			}

			var displayTradePrice = Convert.ToBoolean(session["memberDisplayTradePrice"]);

			var countryMatched = Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]);

			var ppgCache = new ProductPageGridCacheProvider();

			var ppg = ppgCache.Get(dto.PageSectionID,
				dto.LanguageID,
				dto.CountryID,
				tradeAccountId.HasValue,
				tradeAccountId,
				countryMatched,
				useDate);

			if (ppg == null)
			{
				ppg = await Task.FromResult(new cmsLandingPage().loadProductPageGridFromCmsDatabase(dto.PageSectionID, 
					memberId, 
					dto.LanguageID, 
					dto.CountryID, 
					tradeAccountId, 
					dto.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
					displayTradePrice,
					useDate));

				ppgCache.Set(dto.PageSectionID, 
					dto.LanguageID, 
					dto.CountryID, 
					tradeAccountId.HasValue, 
					tradeAccountId,
					dto.MasterDefinition.UrlBreakdown.CountryAwareness.Match, 
					useDate, 
					ppg);
			}

			var filteredProductPages = new List<int>();

			if (dto.Attributes.Any())
			{
				var attributesGrouped = dto.Attributes.GroupBy(a => new { a.AttributeGroupID, a.AttributeOperator }).ToList();

				attributesGrouped.ForEach(ag => 
				{
					var thisGroupProductPageIDs = new List<int>();

					// ag.Key is the AttributeGroupID
					switch(ag.Key.AttributeOperator)
					{
						case "AND":
							thisGroupProductPageIDs.AddRange(ag.Select(a => a.ProductPageIDs).Aggregate((previous, next) => previous.Intersect(next)));
							break;
						case "OR":
							thisGroupProductPageIDs.AddRange(ag.SelectMany(p => p.ProductPageIDs));
							break;
					}

					filteredProductPages.AddRange(thisGroupProductPageIDs);
				});
			}



			// we need a way to use the attributes as given to reduce this potentially enormous list
			// as efficiently as possible
			// as quickly as possible
			//
			// right so now we have ProductPageIDs on each Attribute coming in...
			// and each attribute declares which operator its group uses: AND or OR
			// that means... that means that we need to group them by attribute group
			// and apply 
			//
			// between attribute groups themselves, the operator is always AND

			var preResults = new List<ProductPageFilterablePod>();

			if (filteredProductPages.Any())
			{
				preResults = Framework.StreamingJsonReduction(ppg.JSON, filteredProductPages).ToList();
			}

			var results = preResults.Any() 
					? preResults.Skip(dto.PageSize * (dto.Page - 1)).Take(dto.PageSize).ToList() 
					: Framework.StreamSegmentOfJsonArray(ppg.JSON, "productPages", dto.PageSize * (dto.Page - 1), dto.PageSize).ToList();

			// Datatable to pass to SQL proc
			var productPageToRenderTable = new System.Data.DataTable();

			// Datatable columns... why is Sequence necessary?
			productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
			productPageToRenderTable.Columns.Add("Sequence", typeof(int));

			int sequence = 0;

			// add rows to datatable
			foreach (var productPageId in results)
			{
#if DEBUG
				//System.Diagnostics.Debug.WriteLine(productPage);
#endif
				productPageToRenderTable.Rows.Add(productPageId.ProductPageID, sequence++);
			}

			var objSearchGenerate = new searchGenerate();

			// call SQL render proc
			var productPages = objSearchGenerate.getRenderForSearchResultsAsync(
				tradeAccountId: dto.Visitor.memberTradeAccountID,
				isTrade: dto.MasterDefinition.TradeAllowed,
				displayTradePrice: dto.Visitor.memberDisplayTradePrice,
				productPageToRenderTable,
				useDate,
				dto.MasterDefinition.UrlBreakdown.countryDetails,
				dateOverride: $"{useDate:yyyy-MM-dd}",
				countryOverride: useCountry
			);

			// convert SQL render proc JSON results to List<T>
			var renderedResults = JObject.FromObject(productPages.json)
										.SelectToken("productPages")
										.ToObject<List<ProductPageFilterablePod>>();

			var objDb = new databaseDataContext();

			// call SQL pricing select for page
			var objPrices = objDb.ProductPage_Price_Select(
				results.Select(p => p.ProductPageID),
				dto.Visitor,
				dto.MasterDefinition);

			var serializer = new Newtonsoft.Json.JsonSerializer();

			serializer.ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();

			// convert SQL pricing proc JSON results to List<a'>
			var prices = JObject.FromObject(objPrices.json ?? (new { productPages = new int[] { } }))
				.SelectToken("productPages")
				.Children()
				.Select(p => p.SelectToken("productPage"))
				.Select(p => new
				{
					productPageId = p.SelectToken("productPageId").Value<int>(),
					price = p.SelectToken("productPagePrice").ToObject<ProductPagePodPrice>(serializer)
				}).ToList();

			// Iterate over render results
			for (var i = 0; i < prices.Count(); i++)
			{
				// grab product page id
				var productPageId = prices[i].productPageId;

				// grab index into complete result set 
				var index = results.FindIndex(crs => crs.ProductPageID == productPageId);

				if (index > -1)
				{
					// Set price object for this result
					renderedResults[i].ProductPagePrice = prices.Where(p => p.productPageId == productPageId).Select(p => p.price).SingleOrDefault();

					// Overwrite complete result set at appropriate index with this rendered, priced version
					results[index].ApplyRender(renderedResults[i]);

					// Need to re-establish Product Type as it will have been lost
					results[index].ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == results[index].ProductTypeID);
				}
			}

			var objSearch = new cmsSearch();

			var objSearchResultReturn = await objSearch.loadSearchFromCms(productPageToRenderTable, dto.LanguageID, dto.CountryID, null, tradeAccountId, dto.ProductTypeID, useDate);

			Parallel.ForEach(objSearchResultReturn.searchResultCores, tempResult =>
			{
				var tempSearch = results.Where(p => p.ProductPageID == tempResult.ProductPageID).FirstOrDefault();

				if (tempSearch == null) { }
				else
				{
					tempResult.ScorePercent = tempSearch.ScorePercent;
				}
			});

			objSearchResultReturn.searchResultCores = objSearchResultReturn.searchResultCores.OrderByDescending(p => p.ScorePercent).ThenByDescending(p => p.SearchSortOrder).ThenBy(p => p.ProductPageID).ToList();

			Parallel.ForEach(results, (r) =>
			{
				var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);
				
				if (src != null)
				{
					r.ApplySorting(src);
				}
			});

			switch (dto.Sort)
			{
				case "default":
					return Ok(results.OrderBy(p => p.SortOrder));
				case "bestSellers":
					return Ok(results.OrderBy(p => p.SalesRank));
				case "priceLowToHigh":
					return Ok(results.OrderByDescending(p => p.PriceSortOrder));
				case "priceHighToLow":
					return Ok(results.OrderBy(p => p.PriceSortOrder));
				case "alphaAZ":
					return Ok(results.OrderBy(p => p.AZSortOrder));
				case "alphaZA":
					return Ok(results.OrderByDescending(p => p.AZSortOrder));
				case "new":
					return Ok(results.OrderBy(p => p.NewSort));
				default:
					return Ok(results.OrderBy(p => p.SortOrder));
			}
		}
	}
}