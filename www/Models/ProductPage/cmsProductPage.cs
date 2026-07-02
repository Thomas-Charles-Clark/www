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
using System.Configuration;
using wwwDesignersGuild.Models.Helper;
using Newtonsoft.Json;
using static Antlr.Runtime.Tree.TreeWizard;

namespace wwwDesignersGuild.Models.ProductPage
{
    public class cmsProductPage
    {
        public async Task<cmsProductPageDefinition> loadProductPageFromCms(int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade, bool? platformSwitch)
        {
            cmsProductPageDefinition productPageStore;

            var objProductPageCache = new ProductPageCacheProvider();

            productPageStore = objProductPageCache.GetOrCreate(productPageId, languageId, countryId, isTrade, platformSwitch, useDate, () => {

                var result = Task.Run(() => loadProductPageFromCmsDatabase(productPageId, languageId, countryId, useDate, isTrade, platformSwitch)).Result;

                return result;

            });

            return productPageStore;
        }
        
        private cmsProductPageDefinition loadProductPageFromCmsDatabase(int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade, bool? platformSwitch)
        {
            var dataContext = new databaseDataContext();

            var results = dataContext.ProductPage_Select(productPageId, languageId, countryId, useDate, isTrade, platformSwitch);

            var productPageStore = new cmsProductPageDefinition
            {
                objCacheSelect = results.GetResult<productPageCacheSelect>().SingleOrDefault(),
                objUnavailabilityRedirect = results.GetResult<productPageUnavailableRedirect>().SingleOrDefault()
            };

            return productPageStore;
        }

        public async Task<quickLookReturn> loadQuickLookFromCms(Int64 visitorId, Int32? memberId, int productPageId, short countryId, byte languageId, DateTime useDate, int? memberTradeAccountID)
        {
            quickLookReturn realtimeStockStore;
            realtimeStockStore = await Task.FromResult<quickLookReturn>(loadQuickLookFromCmsDatabase(visitorId, memberId, productPageId, countryId, languageId, useDate, memberTradeAccountID));
            return realtimeStockStore;
        }

        private quickLookReturn loadQuickLookFromCmsDatabase(Int64 visitorId, Int32? memberId, int productPageId, short countryId, byte languageId, DateTime useDate, int? memberTradeAccountID)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.ProductPage_QuickLook_GenerateMultiple(visitorId, memberId, productPageId, countryId, languageId, useDate, memberTradeAccountID);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            quickLookReturn qlr = new quickLookReturn
            {
                prices = results.GetResult<productPageQuickLookPrice>().ToList(),
                products = results.GetResult<productPageQuickLook>().ToList(),
                basketContents = results.GetResult<productPageQuickLookBasketContent>().ToList(),
                texts = results.GetResult<productPageQuickLookText>().ToList()
            };

            return qlr;
        }

        public productPageBundleNotice LoadBundleNoticeFromCms(int productId, byte priceListId, short countryId, byte languageId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            ISingleResult<bundledProduct> results = dataContext.ProductPage_ProductBundle_GenerateSingle(productId, priceListId, countryId, languageId, useDate);

            productPageBundleNotice ppbn = new productPageBundleNotice
            {
                Products = results.ToList()
            };

            return ppbn;
        }

        public async Task<DatabaseJSONResponse> loadTechInfoFromCms(int productPageId, short countryId, byte languageId, DateTime useDate, Int16 CacheTimeMinutes)
        {
            _ = new Models.Database.databaseDataContext();

            ProductPageCacheProvider objProductPageCache = new ProductPageCacheProvider();

            var techInfo = objProductPageCache.GetOrCreateTechInfo(productPageId, languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadTechInfoFromCmsDatabase(productPageId, countryId, languageId, useDate)).Result;

                return result;

            });

            return techInfo;
        }

        private DatabaseJSONResponse loadTechInfoFromCmsDatabase(int productPageId, short countryId, byte languageId,  DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            return dataContext.ProductPage_TechInfo_Select(productPageId, countryId, languageId, useDate);
        }

        public async Task<DatabaseJSONResponse> loadModularInfoFromCms(int productPageId, short countryId, byte languageId, DateTime useDate, Int16 CacheTimeMinutes)
        {
            var objProductPageCache = new ProductPageCacheProvider();

            var modularInfo = objProductPageCache.GetOrCreateModularInfo(productPageId, languageId, countryId, useDate, () => {

                var result = Task.Run(() => loadModularInfoFromCmsDatabase(productPageId, countryId, languageId, useDate)).Result;

                return result;

            });

            return modularInfo;
        }

        private DatabaseJSONResponse loadModularInfoFromCmsDatabase(int productPageId, short countryId, byte languageId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            return dataContext.FetchModularOptions(productPageId, countryId, languageId, useDate);
        }
    }
}