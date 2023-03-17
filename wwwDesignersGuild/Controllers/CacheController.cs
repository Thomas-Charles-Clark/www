using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.Models.Cache;
using System.Diagnostics;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Controllers
{
    public class CacheController : ApiController
    {
        [HttpPost]
        [CacheAuthentication]
        public HttpResponseMessage Purge([FromBody]PassCachePurge objPurge)
        {
            try
            {
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
                        PurgeLandingPage(objPurge);
                        break;
                    case "productPage":
                        PurgeProductPage(objPurge);
                        break;
                    case "collectionPage":
                        PurgeCollectionPage(objPurge);
                        break;
                }
            }
            catch(Exception ex)
            {
                return new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent(ex.ToString())
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
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

        public void PurgeLandingPage(PassCachePurge objPurge)
        {
            var cache = new LandingPageCacheProvider();

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
        }

        public void PurgeProductPage(PassCachePurge objPurge)
        {
            if (objPurge.ExplicitExpiry.HasValue)
            {
                var cmspp = new cmsProductPage();

                if (objPurge.templateId.HasValue)
                {
                    _ = cmspp.invalidateProductPageSetExplicitCacheExpiry(objPurge.templateId.Value, objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.ContentDate, objPurge.TradeSwitch ?? false, objPurge.ExplicitExpiry.Value);
                }
            }
            else
            {
                var cache = new ProductPageCacheProvider();

                switch (objPurge.TradeSwitch)
                {
                    case true:
                    case false:
                        cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, objPurge.TradeSwitch.Value, objPurge.ContentDate);
                        break;
                    case null:
                        cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, false, objPurge.ContentDate);
                        cache.Invalidate(objPurge.ContentID, objPurge.LanguageID, objPurge.CountryID, true, objPurge.ContentDate);
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
