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
using System.Web.Http.Results;

using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Cache.Collection;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Cache.Resources;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Redirect;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
	public class CacheController : ApiController
    {
		[HttpPost]
        [CacheAuthentication]
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

                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, true, null, countryMatch: true, objPurge.ContentDate);
                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, true, null, countryMatch: false, objPurge.ContentDate);
                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, false, null, countryMatch: true, objPurge.ContentDate);
                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID, false, null, countryMatch: true, objPurge.ContentDate);
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
            return Json(MvcApplication.cacheNames);
        }

        [HttpGet]
        public IHttpActionResult Inspect(string cacheName)
        {
			var cache = (MemoryCache)null;

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
                        break;
                    case "Articles":
                        var articlesProvider = new ArticlePageCacheProvider();
                        cache = articlesProvider.Cache;
                        break;
                    case "ArticleTypes":
                        var articleTypesProvider = new ArticleTypeCacheProvider();
                        cache = articleTypesProvider.Cache;
                        break;
                    case "ArticleSubTypes":
                        var articleSubTypesProvider = new ArticleSubTypeCacheProvider();
                        cache = articleSubTypesProvider.Cache;
                        break;
                    case "Banners":
                        var bannersProvider = new BannerCacheProvider();
                        cache = bannersProvider.Cache;
                        break;
                    case "BraintreeErrorResponses":
                        var braintreeErrorResponsesProvider = new BraintreeErrorResponseCacheProvider();
                        cache = braintreeErrorResponsesProvider.Cache;
                        break;
                    case "CareersPage":
                        var careersPageProvider = new CareersPageCacheProvider();
                        cache = careersPageProvider.Cache;
                        break;
                    case "Collections":
                        var collectionsProvider = new CollectionPageCacheProvider();
                        cache = collectionsProvider.Cache;
                        break;
                    case "CollectionProductPageGrids":
                        var collectionProductPageGridProvider = new CollectionProductPageGridCacheProvider();
                        cache = collectionProductPageGridProvider.Cache;
                        break;
                    case "ContactDetails":
                        var contactDetailsProvider = new ContactDetailsCacheProvider();
                        cache = contactDetailsProvider.Cache;
                        break;
                    case "Countries":
                        var countryProvider = new CountryCacheProvider();
                        cache = countryProvider.Cache;
                        break;
                    case "CountryDropDownCountries":
                        var countryDropDownCacheProvider = new CountryDropDownCacheProvider();
                        cache = countryDropDownCacheProvider.Cache;
                        break;
                    case "DotDigitalTrackingBehaviours":
                        var dotDigitalTrackingBehaviourCacheProvider = new DotDigitalTrackingBehaviourCacheProvider();
                        cache = dotDigitalTrackingBehaviourCacheProvider.Cache;
                        break;
                    case "EmailCapturePopupForms":
                        var emailCapturePopupFormsProvider = new EmailCapturePopupFormCacheProvider();
                        cache = emailCapturePopupFormsProvider.Cache;
                        break;
                    case "LandingPages":
                        var landingPagesProvider = new LandingPageCacheProvider();
                        cache = landingPagesProvider.Cache;
                        break;
                    case "LifestyleGalleryPopups":
                        var lifestyleGalleryPopupsProvider = new LifestyleGalleryPopupCacheProvider();
                        cache = lifestyleGalleryPopupsProvider.Cache;
                        break;
                    case "Hotspots":
                        var hotspotsProvider = new HotspotCacheProvider();
                        cache = hotspotsProvider.Cache;
                        break;
                    case "MobileDetections":
                        var mobileDetectionsProvider = new MobileDetectionCacheProvider();
                        cache = mobileDetectionsProvider.Cache;
                        break;
                    case "Menus":
                        var menusProvider = new MenuCacheProvider();
                        cache = menusProvider.Cache;
                        break;
                    case "ProductPages":
                        var productPagesProvider = new ProductPageCacheProvider();
                        cache = productPagesProvider.Cache;
                        break;
                    case "ProductPageGrids":
                        var productPageGridsProvider = new ProductPageGridCacheProvider();
                        cache = productPageGridsProvider.Cache;
                        break;
                    case "ProductTypeOrderRestrictions":
                        var productTypeOrderRestrictionCacheProvider = new ProductTypeOrderRestrictionCacheProvider();
                        cache = productTypeOrderRestrictionCacheProvider.Cache;
                        break;
                    case "Resources":
                        var resourcesCacheProvider = new ResourceCacheProvider();
                        cache = resourcesCacheProvider.Cache;
                        break;
                    case "Redirects":
                        var redirectsCacheProvider = new RedirectCacheProvider();
                        cache = redirectsCacheProvider.Cache;
                        break;
                    case "Scrapbooks":
                        var scrapbookCacheProvider = new ScrapbookCacheProvider();
                        cache = scrapbookCacheProvider.Cache;
                        break;
                    case "SearchProductTypes":
                        var searchProductTypeCacheProvider = new SearchProductTypeDropDownCacheProvider();
                        cache = searchProductTypeCacheProvider.Cache;
                        break;
                    case "SiteSections":
                        var siteSectionsCacheProvider = new SiteSectionCacheProvider();
                        cache = siteSectionsCacheProvider.Cache;
                        break;
                    case "Snippets":
                        var snippetsCacheProvider = new SnippetCacheProvider();
                        cache = snippetsCacheProvider.Cache;
                        break;
                    case "StockStatuses":
                        var stockStatusesCacheProvider = new StockStatusCacheProvider();
                        cache = stockStatusesCacheProvider.Cache;
                        break;
                    case "TradeAccountRequestForms":
                        var tradeAccountRequestFormsCacheProvider = new TradeAccountRequestFormCacheProvider();
                        cache = tradeAccountRequestFormsCacheProvider.Cache;
                        break;
                    case "UnsubscribePages":
                        var unsubscribePagesCacheProvider = new UnsubscribePageCacheProvider();
                        cache = unsubscribePagesCacheProvider.Cache;
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
                            using (var streamWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8, 2048, leaveOpen: true))
                            using (var jsonWriter = new JsonTextWriter(streamWriter))
                            {
                                await jsonWriter.WriteStartArrayAsync();

                                // Pass the targeted cache instance explicitly to the streaming function
                                foreach (var entry in CacheMetadataExtractor.GetActiveEntriesMetadata(cache))
                                {
                                    var token = JToken.FromObject(entry);
                                    await token.WriteToAsync(jsonWriter);

                                    await jsonWriter.FlushAsync();
                                    await streamWriter.FlushAsync();
                                    await outputStream.FlushAsync();
                                }

                                await jsonWriter.WriteEndArrayAsync();
                                await jsonWriter.FlushAsync();
                                await streamWriter.FlushAsync();
                                await outputStream.FlushAsync();
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
    }
}