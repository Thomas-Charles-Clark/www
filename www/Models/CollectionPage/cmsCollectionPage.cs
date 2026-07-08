using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Helper;
using NLog;
using wwwDesignersGuild.Models.Cache.Collection;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.CollectionPage
{
    public class cmsCollectionPage
    {
        public async Task<cmsCollectionPageDefinition> loadCollectionPageFromCms(short collectionId, byte languageId, short countryId, int? memberId, int? tradeAccountId, bool displayTradePrice, bool countryMatch, DateTime useDate, bool isTrade)
        {

            //  Is collection in cache?
            cmsCollectionPageDefinition collectionPageStore;

            CollectionPageCacheProvider objCollectionPageCache = new CollectionPageCacheProvider();

            collectionPageStore = objCollectionPageCache.GetOrCreate(collectionId, languageId, countryId, useDate, isTrade, () => {

                var result = Task.Run(() => loadCollectionPageFromCmsDatabase(collectionId, languageId, countryId, useDate, isTrade)).Result;

                return result;

            });

            var ppCache = new CollectionProductPageGridCacheProvider();

            var ppJson = ppCache.GetOrCreate(collectionId, languageId, countryId, isTrade, tradeAccountId, countryMatch, useDate, () => {

                var result = loadCollectionProductPageGridFromCmsDatabase(collectionId, memberId, languageId, countryId, tradeAccountId, displayTradePrice, useDate, countryMatch);

                return result;

			});

            collectionPageStore.productPageJson = ppJson;

            return collectionPageStore;

        }

        private cmsCollectionPageDefinition loadCollectionPageFromCmsDatabase(short collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.Collection_GenerateMultiple(collectionId, languageId, countryId, useDate, isTrade);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsCollectionPageDefinition collectionStore = new cmsCollectionPageDefinition
            {
                CollectionDetails = results.GetResult<collectionSelect>().SingleOrDefault(),
                collectionImageSelects = results.GetResult<collectionImageSelect>().ToList<collectionImageSelect>(),
                collectionRelatedCollectionSelects = results.GetResult<collectionRelatedCollectionSelect>().ToList<collectionRelatedCollectionSelect>(),
                collectionBrochureSelects = results.GetResult<collectionBrochureSelect>().ToList<collectionBrochureSelect>(),
                collectionVideos = results.GetResult<collectionVideo>().ToList<collectionVideo>(),
                collectionGoogleHrefs = results.GetResult<GoogleHref>().ToList<GoogleHref>(),
                collectionPageBreadcrumbs = results.GetResult<collectionBreadcrumb>().ToList(),
                objUnavailabilityRedirect = results.GetResult<collectionUnavailabilityRedirect>().SingleOrDefault(),
            };

            return collectionStore;
        }

        private PageSectionJSON loadCollectionProductPageGridFromCmsDatabase(short collectionId, int? memberId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, DateTime useDate, bool countryCheckMatched)
        {
            using (var dataContext = new databaseDataContext())
            {
                return dataContext.FetchCollectionProductPageGrid(collectionId, memberId, languageId, countryId, tradeAccountId, displayTradePrice, useDate, countryCheckMatched);
            }
        }
    }
}