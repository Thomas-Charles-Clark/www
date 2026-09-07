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
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.LandingPage
{
    public class cmsLandingPage : ICmsLandingPageRepository
    {
        public async Task<byte?> loadSiteSectionFromCms(int pageId, DateTime useDate)
        {
            byte? siteSectionId = null;

            var cache = new SiteSectionCacheProvider();
             
            siteSectionId = cache.GetOrCreate(pageId, useDate, () => {

                var result = Task.Run(() => loadSiteSectionFromCmsDatabase(pageId)).Result;

                return result;

            });

            return siteSectionId;
        }
        
        private byte? loadSiteSectionFromCmsDatabase(int pageId)
        {
            using (var dataContext = new databaseDataContext())
            {
                return dataContext.FetchSiteSection(pageId);
            }
        }

        public async Task<cmsLandingPageDefinition> loadLandingPageFromCms(int? memberId, 
            int pageId, 
            byte languageId, 
            short countryId, 
            DateTime useDate, 
            bool isMember, 
            bool isTrade, 
            bool countryMatch, 
            bool displayTradePrice, 
            int? tradeAccountId,
            int? productPageGridPage,
            int? productPageGridPageSize,
            HttpResponseBase responseBase)
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            //  Is page in cache?
            cmsLandingPageDefinition landingPageStore;
            LandingPageCacheProvider objLandingPageCache = new LandingPageCacheProvider();
            
            landingPageStore = objLandingPageCache.GetOrCreate(pageId, languageId, countryId, useDate, isTrade, responseBase, () => {
                var result = Task.Run(() => loadLandingPageFromCmsDatabase(memberId, pageId, languageId, countryId, useDate, isMember, isTrade, displayTradePrice, tradeAccountId)).Result;
                return result;
            });

            // so with the product page grid section page section ids...
            var productPageGridSections = landingPageStore.pageSectionSelects.Where(p => p.PageSectionTypeID == 4).Select(p => p.PageSectionID).ToList();

            var ppgCache = new ProductPageGridCacheProvider();

            var ppgs = new List<PageSectionJSON>();

            foreach (var pageSectionId in productPageGridSections)
            {
                var ppg = ppgCache.GetOrCreate(pageSectionId, languageId, countryId, isTrade, tradeAccountId, countryMatch, useDate, () => {

                    var result = Task.Run(() => loadProductPageGridFromCmsDatabase(pageSectionId, memberId, languageId, countryId, tradeAccountId, countryMatch, displayTradePrice, useDate)).Result;

                    return result;

                });

                ppgs.Add(ppg);
            }

            landingPageStore.productPageGridSections = ppgs;

            return landingPageStore;
        }

        internal PageSectionJSON loadProductPageGridFromCmsDatabase(long pageSectionId, int? memberId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool displayTradePrice, DateTime useDate)
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
                    PageDetails = results.GetResult<PageLanguageSelect>().SingleOrDefault(),
                    pageSectionSelects = results.GetResult<PageSectionSelect>().ToList(),
                    pageSectionHtmls = results.GetResult<PageSectionHTML>().ToList(),
                    pageSectionSlideShowAspects = results.GetResult<PageSectionSlideShowAspect>().ToList(),
                    pageSectionSlideShows = results.GetResult<PageSectionSlideShow>().ToList(),
                    pageSectionForms = results.GetResult<PageSectionForm>().ToList(),
                    pageSectionFormRetailAppointmentAgendas = results.GetResult<PageSectionFormRetailAppointmentAgenda>().ToList(),
                    pageSectionFormRetailAppointmentTypes = results.GetResult<PageSectionFormRetailAppointmentType>().ToList(),
                    pageSectionCollections = results.GetResult<PageSectionCollection>().ToList(),
                    pageSectionCollectionImages = results.GetResult<PageSectionCollectionImage>().ToList(),
                    pageSectionPageSelects = results.GetResult<PageSectionPageSelect>().ToList(),
                    pageSectionPageSelectImages = results.GetResult<PageSectionPageSelectImage>().ToList(),
                    pageSectionBanners = results.GetResult<PageSectionBannerJSON>().Single(),
                    pageRelateds = results.GetResult<PageRelated>().ToList(),
                    Breadcrumbs = results.GetResult<Breadcrumb>().ToList(),
                    pageGoogleHrefs = results.GetResult<GoogleHref>().ToList(),
                    pageSectionArticles = results.GetResult<PageSectionArticle>().ToList(),
                    pageSectionArticleImages = results.GetResult<PageSectionArticleImage>().ToList(),
                    pageFormCountrys = results.GetResult<PageFormCountry>().ToList(),
                    pageSectionVideos = results.GetResult<PageSectionVideo>().ToList(),
                    pageSectionStoreEvents = results.GetResult<PageSectionStoreEvent>().ToList(),
                    pageSectionAnimations = results.GetResult<PageSectionAnimation>().ToList(),
                    pageSectionAnimationSlides = results.GetResult<PageSectionAnimationSlide>().ToList(),
                    pageSectionAnimationSlideCallsToAction = results.GetResult<PageSectionAnimationSlideCallToAction>().ToList(),
                    ratingAggregate = results.GetResult<PageRatingAggregate>().ToList()
                };
                return pageStore;
            }
        }
    }
}