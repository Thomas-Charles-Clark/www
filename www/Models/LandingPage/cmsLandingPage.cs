using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Interface;
using System.Data.SqlClient;
using System.Configuration;
using wwwDesignersGuild.Models.Cache.ProductPages;
using System.Management.Automation;

namespace wwwDesignersGuild.Models.LandingPage
{
    public class cmsLandingPage : ICmsLandingPageRepository
    {
        public async Task<byte?> loadSiteSectionFromCms(int pageId, DateTime useDate)
        {
            byte? result = null;

            var cache = new SiteSectionCacheProvider();
             
            result = cache.Get(pageId, useDate);

            if (result == null) // neat trick! all landing pages have a site section, so this stands.
            {
                result = await Task.FromResult(loadSiteSectionFromCmsDatabase(pageId));

                if (result != null)
                {
                    cache.Set(pageId, useDate, result);
                }
            }

            return result;
        }
        
        private byte? loadSiteSectionFromCmsDatabase(int pageId)
        {
            using (var dataContext = new databaseDataContext())
            {
                return dataContext.FetchSiteSection(pageId);
            }
        }

        public async Task<cmsLandingPageDefinition> loadLandingPageFromCms(int? memberId, int pageId, byte languageId, short countryId, DateTime useDate, bool isMember, bool isTrade, bool countryMatch, bool displayTradePrice, int? tradeAccountId)
        {
            //  Is page in cache?
            cmsLandingPageDefinition landingPageStore;
            LandingPageCacheProvider objLandingPageCache = new LandingPageCacheProvider();
            
            landingPageStore = objLandingPageCache.Get(pageId, languageId, countryId, useDate, isTrade);

            if (landingPageStore == null)
            {
                landingPageStore = await Task.FromResult(loadLandingPageFromCmsDatabase(memberId, pageId, languageId, countryId, useDate, isMember, isTrade, displayTradePrice, tradeAccountId));

                var stockStatus = new cmsStockStatus();
                var stockStatuses = await stockStatus.loadStockStatusesFromCms(languageId, countryId, useDate);

                //landingPageStore.pageSectionProductPages.ForEach(c => 
                //{
                //    c.StockStatus = stockStatuses.StockStatuses.Where(p => p.StockStatusID == c.StockStatusID).SingleOrDefault()?.StockStatus;
                //});
            
                objLandingPageCache.Set(pageId, languageId, countryId, useDate, isTrade, landingPageStore);
            }

            // so with the product page grid section page section ids...
            var productPageGridSections = landingPageStore.pageSectionSelects.Where(p => p.PageSectionTypeID == 4).Select(p => p.PageSectionID).ToList();

            var ppgCache = new ProductPageGridCacheProvider();

            var ppgs = new List<PageSectionJSON>();

            foreach (var pageSectionId in productPageGridSections)
            {
                var ppg = ppgCache.Get(pageSectionId, languageId, countryId, isTrade, tradeAccountId, countryMatch, useDate);

                if (ppg == null)
                {
                    ppg = await Task.FromResult(loadProductPageGridFromCmsDatabase(pageSectionId, memberId, languageId, countryId, tradeAccountId, countryMatch, displayTradePrice, useDate));

                    ppgCache.Set(pageSectionId, languageId, countryId, isTrade, tradeAccountId, countryMatch, useDate, ppg);
                }

                ppgs.Add(ppg);
            }

            landingPageStore.productPageGridSections = ppgs;

            return landingPageStore;
        }

        private PageSectionJSON loadProductPageGridFromCmsDatabase(long pageSectionId, int? memberId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool displayTradePrice, DateTime useDate)
        {
            using (var dataContext = new databaseDataContext())
            {
                return dataContext.FetchProductPageGrid(pageSectionId, memberId, languageId, countryId, tradeAccountId, countryMatch, displayTradePrice, useDate);
            }
        }

        private cmsLandingPageDefinition loadLandingPageFromCmsDatabase(int? memberId, int pageId, byte languageId, short countryId, DateTime useDate, Boolean isMember, Boolean isTrade, bool displayTradePrice, int? tradeAccountId)
        {
            using (var dataContext = new databaseDataContext())
            {
                IMultipleResults results = dataContext.Page_GenerateMultiple(pageId, languageId, countryId, useDate, isMember, isTrade, memberId, displayTradePrice, tradeAccountId);

                cmsLandingPageDefinition pageStore = new cmsLandingPageDefinition
                {
                    //  Convert from IEnumerable to a List so a collection query can be run multiple times
                    PageDetails = results.GetResult<pageLanguageSelect>().SingleOrDefault(),
                    pageSectionSelects = results.GetResult<pageSectionSelect>().ToList(),
                    pageSectionHtmls = results.GetResult<pageSectionHtml>().ToList(),
                    pageSectionSlideShowAspects = results.GetResult<pageSectionSlideShowAspect>().ToList(),
                    pageSectionSlideShows = results.GetResult<pageSectionSlideShow>().ToList(),
                    pageSectionForms = results.GetResult<pageSectionForm>().ToList(),
                    pageSectionFormRetailAppointmentAgendas = results.GetResult<pageSectionFormRetailAppointmentAgenda>().ToList(),
                    pageSectionFormRetailAppointmentTypes = results.GetResult<pageSectionFormRetailAppointmentType>().ToList(),
                    pageSectionCollections = results.GetResult<pageSectionCollection>().ToList(),
                    pageSectionCollectionImages = results.GetResult<pageSectionCollectionImage>().ToList(),
                    pageSectionPageSelects = results.GetResult<pageSectionPageSelect>().ToList(),
                    pageSectionPageSelectImages = results.GetResult<pageSectionPageSelectImage>().ToList(),
                    pageSectionBanners = results.GetResult<pageSectionBannerJson>().Single(),
                    pageRelateds = results.GetResult<pageRelated>().ToList(),
                    Breadcrumbs = results.GetResult<Breadcrumb>().ToList(),
                    pageGoogleHrefs = results.GetResult<googleHref>().ToList(),
                    pageSectionArticles = results.GetResult<pageSectionArticle>().ToList(),
                    pageSectionArticleImages = results.GetResult<pageSectionArticleImage>().ToList(),
                    pageFormCountrys = results.GetResult<pageFormCountry>().ToList(),
                    pageSectionVideos = results.GetResult<pageSectionVideo>().ToList(),
                    pageSectionStoreEvents = results.GetResult<pageSectionStoreEvent>().ToList(),
                    pageSectionAnimations = results.GetResult<pageSectionAnimation>().ToList(),
                    pageSectionAnimationSlides = results.GetResult<pageSectionAnimationSlide>().ToList(),
                    pageSectionAnimationSlideCallsToAction = results.GetResult<pageSectionAnimationSlideCallToAction>().ToList(),
                    ratingAggregate = results.GetResult<pageRatingAggregate>().ToList()
                };
                return pageStore;
            }
        }
    }
}