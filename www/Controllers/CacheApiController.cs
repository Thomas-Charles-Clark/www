using DocumentFormat.OpenXml.Vml.Spreadsheet;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Cache.Collection;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Cache.Resources;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Redirect;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
	[CacheAuthentication]
	public class CacheApiController : ApiController
    {
		[HttpPost]
        public async Task<IHttpActionResult> Purge([FromBody]PassCachePurge objPurge)
        {
            try
            {
                if (objPurge.CountryURL == null)
                {
                    var countries = new cmsCountry();

                    var countryLanguages = await countries.countryLanguageSelect(objPurge.ContentDate);

                    objPurge.CountryURL = countryLanguages.countryItems.FirstOrDefault(cl => cl.CountryID == objPurge.CountryID && cl.LanguageID == objPurge.LanguageID)?.URL;
                }

                switch (objPurge.ContentType)
                {
                    case "banner":
                        PurgeBanner(objPurge);
                        break;
                    case "article":
                        PurgeArticle(objPurge);
                        break;
                    case "careers":
                        PurgeCareers(objPurge);
                        break;
                    case "countries":
                        PurgeCountries(objPurge);
                        break;
                    case "productTypes":
                        PurgeProductTypes(objPurge);
                        break;
                    case "emailCapturePopup":
                        PurgeEmailCapture(objPurge);
                        break;
                    case "snippet":
                        PurgeSnippet(objPurge);
                        break;
                    case "menu":
                        PurgeMenu(objPurge);
                        break;
                    case "productPageAttributes":
                        PurgeProductPageAttributes(objPurge);
                        break;
                    case "stockStatuses":
                        PurgeStockStatuses(objPurge);
                        break;
                    case "resources":
                        PurgeResources(objPurge);
                        break;
                    case "landingPage":
                        return Ok(new {
                            productPageGridPurgeDetails = PurgeLandingPage(objPurge)
                        });
                    case "productPage":
                        PurgeProductPage(objPurge);
                        PurgeSnippet(objPurge);
                        break;
                    case "collectionPage":
                        PurgeCollectionPage(objPurge);
                        break;
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok(new { });
        }

		public void PurgeBanner(PassCachePurge objPurge)
        {
            var cache = new BannerCacheProvider();

            switch (objPurge.TradeSwitch)
            {
                case true:
                case false:
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, objPurge.TradeSwitch.Value, false);
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, objPurge.TradeSwitch.Value, true);
                    break;
                case null:
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, false, false);
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, true, true);
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, false, true);
                    cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate, true, false);
                    break;
            }
        }

        public void PurgeCareers(PassCachePurge objPurge)
        {
            var cache = new CareersPageCacheProvider();
            
            cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        public void PurgeCountries(PassCachePurge objPurge)
        {
            var cache = new CountryCacheProvider();
            var cache2 = new CountryDropDownCacheProvider();

            cache.Invalidate();
            cache2.Invalidate();
        }

        public void PurgeProductTypes(PassCachePurge objPurge)
        {
            // SearchProductType 
            // ProductTypeOrderRestriction
            var cache = new SearchProductTypeDropDownCacheProvider();
            var cache2 = new ProductTypeOrderRestrictionCacheProvider();

            cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate);
            cache2.Invalidate(objPurge.TradeAccountID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        public void PurgeEmailCapture(PassCachePurge objPurge)
        {
            var cache = new EmailCapturePopupFormCacheProvider();

            cache.Invalidate(objPurge.CountryID, objPurge.LanguageID, objPurge.ContentDate);
        }

        public void PurgeSnippet(PassCachePurge objPurge)
        {
            var cache = new SnippetCacheProvider();

            cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        public void PurgeMenu(PassCachePurge objPurge)
        {
            var cache = new MenuCacheProvider();

            switch (objPurge.TradeSwitch)
            {
                case true:
                case false:
                    cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, objPurge.TradeSwitch.Value);
                    break;
                case null:
                    cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, true);
                    cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, false);
                    break;
            }
        }

        public void PurgeProductPageAttributes(PassCachePurge objPurge)
        {
            var cache = new AttributeCacheProvider();

            cache.Invalidate(objPurge.LanguageID);
        }

        public void PurgeStockStatuses(PassCachePurge objPurge)
        {
            var cache = new StockStatusCacheProvider();

            cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        public void PurgeResources(PassCachePurge objPurge)
        {
            var cache = new ResourceCacheProvider();

            cache.Invalidate(objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        public IEnumerable<string> PurgeLandingPage(PassCachePurge objPurge)
        {
            var cache = new LandingPageCacheProvider();

            var productPageGridCache = new ProductPageGridCacheProvider();

            switch (objPurge.TradeSwitch)
            {
                case true:
                case false:
                    cache.Invalidate(Convert.ToInt32(objPurge.ContentID), objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, objPurge.TradeSwitch.Value);
                    break;
                case null:
                    cache.Invalidate(Convert.ToInt32(objPurge.ContentID), objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, false);
                    cache.Invalidate(Convert.ToInt32(objPurge.ContentID), objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, true);
                    break;
            }

            // We must pass all the PageSections belonging to this LandingPage that we wish to purge from the cache
            var results = new List<string>();

            foreach (var ps in objPurge.PageSectionIDs)
            {
                //results.Add(productPageGridCache.InvalidateMultiple(ps, objPurge.LanguageID, objPurge.CountryID));

                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, false, null, true, objPurge.ContentDate);
                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, false, null, false, objPurge.ContentDate);
            }

            return results;
        }

        public void PurgeProductPage(PassCachePurge objPurge)
        {
            var cache = new ProductPageCacheProvider();

            switch (objPurge.TradeSwitch)
            {
                case true:
                case false:
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.TradeSwitch.Value, objPurge.PlatformSwitch, objPurge.ContentDate);
                    break;
                case null:
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, false, objPurge.PlatformSwitch, objPurge.ContentDate);
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, true, objPurge.PlatformSwitch, objPurge.ContentDate);
                    break;
            }
        }

        public void PurgeCollectionPage(PassCachePurge objPurge)
        {
            var cache = new CollectionPageCacheProvider();

            switch (objPurge.TradeSwitch)
            {
                case true:
                case false:
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, objPurge.TradeSwitch.Value);
                    break;
                case null:
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, false);
                    cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, true);
                    break;
            }
        }

        public void PurgeArticle(PassCachePurge objPurge)
        {
            var cache = new ArticlePageCacheProvider();

            cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate);
        }

        [HttpGet]
        public IHttpActionResult List()
        {
            return Ok(JsonConvert.SerializeObject(MvcApplication.cacheNames, Formatting.Indented));
        }

        [HttpGet]
        public IHttpActionResult Inspect(string cacheName)
        {
            var cache = (MemoryCache)null;
            var keyBreakdown = (string[])null;

            if (!MvcApplication.cacheNames.Contains(cacheName))
            {
                throw new Exception($"Cache Name Not Found: \"{cacheName}\"");
            }
            else
            {
                switch (cacheName)
                {
                    case "Attributes":
                        var attributesProvider = new AttributeCacheProvider();
                        cache = attributesProvider.Cache;
						keyBreakdown = new string[] { "LanguageID" };
						break;
                    case "Articles":
                        var articlesProvider = new ArticlePageCacheProvider();
                        cache = articlesProvider.Cache;
                        keyBreakdown = new string[] { "ArticleID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "ArticleTypes":
                        var articleTypesProvider = new ArticleTypeCacheProvider();
                        cache = articleTypesProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "ArticleSubTypes":
                        var articleSubTypesProvider = new ArticleSubTypeCacheProvider();
                        cache = articleSubTypesProvider.Cache;
                        keyBreakdown = new string[] { "ArticleTypeID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "Banners":
                        var bannersProvider = new BannerCacheProvider();
                        cache = bannersProvider.Cache;
                        keyBreakdown = new string[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch", "PlatformSwitch" };
						break;
                    case "BraintreeErrorResponses":
                        var braintreeErrorResponsesProvider = new BraintreeErrorResponseCacheProvider();
                        cache = braintreeErrorResponsesProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "CareersPage":
                        var careersPageProvider = new CareersPageCacheProvider();
                        cache = careersPageProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "Collections":
                        var collectionsProvider = new CollectionPageCacheProvider();
                        cache = collectionsProvider.Cache;
                        keyBreakdown = new string[] { "CollectionID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" };
						break;
                    case "CollectionProductPageGrids":
                        var collectionProductPageGridProvider = new CollectionProductPageGridCacheProvider();
                        cache = collectionProductPageGridProvider.Cache;
                        keyBreakdown = new string[] { "CollectionID", "LanguageID", "CountryID", "TradeSwitch", "TradeAccountID", "CountryMatched", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "ContactDetails":
                        var contactDetailsProvider = new ContactDetailsCacheProvider();
                        cache = contactDetailsProvider.Cache;
                        keyBreakdown = new string[] { "TradeAccountID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "Countries":
                        var countryProvider = new CountryCacheProvider();
                        cache = countryProvider.Cache;
                        keyBreakdown = new string[] { };
						break;
                    case "CountryDropDownCountries":
                        var countryDropDownCacheProvider = new CountryDropDownCacheProvider();
                        cache = countryDropDownCacheProvider.Cache;
                        keyBreakdown = new string[] { };
						break;
                    case "DotDigitalTrackingBehaviours":
                        var dotDigitalTrackingBehaviourCacheProvider = new DotDigitalTrackingBehaviourCacheProvider();
                        cache = dotDigitalTrackingBehaviourCacheProvider.Cache;
                        keyBreakdown = new string[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "EmailCapturePopupForms":
                        var emailCapturePopupFormsProvider = new EmailCapturePopupFormCacheProvider();
                        cache = emailCapturePopupFormsProvider.Cache;   
                        keyBreakdown = new string[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "LandingPages":
                        var landingPagesProvider = new LandingPageCacheProvider();
                        cache = landingPagesProvider.Cache;
                        keyBreakdown = new string[] { "PageID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" };
                        break;
                    case "LifestyleGalleryPopups":
                        var lifestyleGalleryPopupsProvider = new LifestyleGalleryPopupCacheProvider();
                        cache = lifestyleGalleryPopupsProvider.Cache;
                        keyBreakdown = new string[] { "ProductPageID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "Hotspots":
                        var hotspotsProvider = new HotspotCacheProvider();
                        cache = hotspotsProvider.Cache;
                        keyBreakdown = new string[] { "ContentID", "ContentType", "TradeAccountID", "DisplayTradePrice", "CountryID", "LanguageID", "Date", "ImageIDs", "CountryMatch" };
                        break;
                    case "MobileDetections":
                        var mobileDetectionsProvider = new MobileDetectionCacheProvider();
                        cache = mobileDetectionsProvider.Cache;
                        keyBreakdown = new string[] { "UserAgent" };
                        break;
                    case "Menus":
                        var menusProvider = new MenuCacheProvider();
                        cache = menusProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day", "TradeSwitch" };
                        break;
                    case "ProductPages":
                        var productPagesProvider = new ProductPageCacheProvider();
                        cache = productPagesProvider.Cache;
                        keyBreakdown = new string[] { "ProductPageID", "LanguageID", "CountryID", "TradeSwitch", "PlatformSwitch", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "ProductPageGrids":
                        var productPageGridsProvider = new ProductPageGridCacheProvider();
                        cache = productPageGridsProvider.Cache;
                        keyBreakdown = new string[] { "PageSectionID", "LanguageID", "CountryID", "TradeSwitch", "TradeAccountID", "CountryMatch", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "ProductTypeOrderRestrictions":
                        var productTypeOrderRestrictionCacheProvider = new ProductTypeOrderRestrictionCacheProvider();
                        cache = productTypeOrderRestrictionCacheProvider.Cache;
                        keyBreakdown = new string[] { "TradeAccountID", "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "Resources":
                        var resourcesCacheProvider = new ResourceCacheProvider();
                        cache = resourcesCacheProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID" };
                        break;
                    case "Redirects":
                        var redirectsCacheProvider = new RedirectCacheProvider();
                        cache = redirectsCacheProvider.Cache;
                        keyBreakdown = new string[] { "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "Scrapbooks":
                        var scrapbookCacheProvider = new ScrapbookCacheProvider();
                        cache = scrapbookCacheProvider.Cache;
                        keyBreakdown = new string[] { "VisitorID", "MemberID", "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "SearchProductTypes":
                        var searchProductTypeCacheProvider = new SearchProductTypeDropDownCacheProvider();
                        cache = searchProductTypeCacheProvider.Cache;
                        keyBreakdown = new string[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "SiteSections":
                        var siteSectionsCacheProvider = new SiteSectionCacheProvider();
                        cache = siteSectionsCacheProvider.Cache;
						keyBreakdown = new string[] { "PageID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "Snippets":
                        var snippetsCacheProvider = new SnippetCacheProvider();
                        cache = snippetsCacheProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "StockStatuses":
                        var stockStatusesCacheProvider = new StockStatusCacheProvider();
                        cache = stockStatusesCacheProvider.Cache;
                        keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
                        break;
                    case "TradeAccountRequestForms":
                        var tradeAccountRequestFormsCacheProvider = new TradeAccountRequestFormCacheProvider();
                        cache = tradeAccountRequestFormsCacheProvider.Cache;
						keyBreakdown = new string[] { "CountryID", "LanguageID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    case "UnsubscribePages":
                        var unsubscribePagesCacheProvider = new UnsubscribePageCacheProvider();
                        cache = unsubscribePagesCacheProvider.Cache;
						keyBreakdown = new string[] { "LanguageID", "CountryID", "Date.Year", "Date.Month", "Date.Day" };
						break;
                    default:
                        cache = null;
                        break;
                }

                if (cache != null && cache.Any())
                {
					var response = new HttpResponseMessage(HttpStatusCode.OK);

					response.Content = new PushStreamContent(async (outputStream, httpContent, transportContext) =>
					{
						try
						{
							// Set an efficient, larger internal stream buffer (8KB)
							using (var streamWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8, 8192, leaveOpen: true))
							using (var jsonWriter = new JsonTextWriter(streamWriter))
							{
								var serializer = new JsonSerializer();
								await streamWriter.WriteAsync("["); // Manually stream the array bounds safely

								bool isFirst = true;

								// Active entries are now streamed dynamically straight out of the internal cache reflection!
								foreach (var entry in CacheMetadataExtractor.GetActiveEntriesMetadata(cache))
								{
									if (!isFirst)
									{
										await streamWriter.WriteAsync(",");
									}
									isFirst = false;

									entry.KeyBreakdown = keyBreakdown;

									// Stream directly to the output wire with zero temporary intermediate allocations
									serializer.Serialize(jsonWriter, entry);
								}

								await streamWriter.WriteAsync("]");
								await streamWriter.FlushAsync();
							}
						}
						finally
						{
							outputStream.Close();
						}
					}, new MediaTypeHeaderValue("application/json"));

					return new ResponseMessageResult(response);
				}
                else
                { 
                    return Json("[]");
                }
            }
        }


		[HttpGet]
		public IHttpActionResult Summarise(string cacheName)
		{
			// 1. Guard check and resolve the live global cache singleton using your preferred indexing
			if (string.IsNullOrWhiteSpace(cacheName) || !MvcApplication.CacheSingletons.TryGetValue(cacheName, out var cache))
			{
				return BadRequest($"Cache Name Not Found: \"{cacheName}\"");
			}

			// 2. Fetch the metadata schema array instantly from the parallel lookup table
			if (!MvcApplication.CacheKeySchemas.TryGetValue(cacheName, out var keyBreakdown))
			{
				keyBreakdown = Array.Empty<string>(); // Clean fallback if a name exists but lacks a formal breakdown
			}

			// 3. Process the state summary via our memory-safe extraction framework
			var summary = CacheMetadataExtractor.GetCacheStateSummary(cacheName, keyBreakdown, cache);

			if (summary == null)
			{
				return NotFound();
			}

			return Ok(summary);
		}
	}
}