using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.Models.Cache;
using System.Diagnostics;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models;
using System.Collections.Generic;
using wwwDesignersGuild.Models.Country;
using System.Threading.Tasks;
using System.Linq;

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

            cache.Invalidate(objPurge.CountryID, objPurge.LanguageID);
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

                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID,  null, true, objPurge.ContentDate);
                productPageGridCache.Invalidate(ps, objPurge.LanguageID, objPurge.CountryID,  null, false, objPurge.ContentDate);
            }

            return results;
        }

        public void PurgeProductPage(PassCachePurge objPurge)
        {
            if (objPurge.ExplicitExpiry.HasValue)
            {
                var cmspp = new cmsProductPage();

                if (objPurge.templateId.HasValue)
                {
                    _ = cmspp.invalidateProductPageSetExplicitCacheExpiry(objPurge.templateId.Value, objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, objPurge.TradeSwitch ?? false, dgFramework.IsMobileDevice(), objPurge.ExplicitExpiry.Value);
                }
            }
            else
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
    }
}
 