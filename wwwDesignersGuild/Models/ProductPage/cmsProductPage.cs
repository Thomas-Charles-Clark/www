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


    public class cmsProductPage : ICmsProductPageRepository
    {


        public async Task<cmsProductPageDefinition> loadProductPageFromCms(int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            //  Is page in cache?
            cmsProductPageDefinition productPageStore;
            ProductPageCacheProvider objProductPageCache = new ProductPageCacheProvider();
            productPageStore = objProductPageCache.Get(productPageId, languageId, countryId, isTrade, useDate);

            if (productPageStore == null)
            {
                productPageStore = await Task.FromResult<cmsProductPageDefinition>(loadProductPageFromCmsDatabase(productPageId, languageId, countryId, useDate, isTrade));

                objProductPageCache.Set(productPageId, languageId, countryId, isTrade, useDate, productPageStore);
            }

            return productPageStore;
        }

        public async Task<cmsProductPageDefinition> invalidateProductPageSetExplicitCacheExpiry(byte templateId, int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade, DateTimeOffset expiration)
        {
            var objProductPageCache = new ProductPageCacheProvider();
            
            objProductPageCache.Invalidate(productPageId, languageId, countryId, isTrade, useDate);

            var productPageStore = await Task.FromResult<cmsProductPageDefinition>(loadProductPageFromCmsDatabase(productPageId, languageId, countryId, useDate, isTrade));

            objProductPageCache.ExplicitSet(productPageId, languageId, countryId, isTrade, useDate, productPageStore, expiration);

            if (templateId == 2)
            {
                objProductPageCache.InvalidateTechInfo(productPageId, languageId, countryId, useDate);
                var techInfo = await Task.FromResult(loadTechInfoFromCmsDatabase(productPageId, countryId, languageId, useDate));
                objProductPageCache.ExplicitSetTechInfo(productPageId, languageId, countryId, useDate, techInfo, expiration);
            }

            if (templateId == 5)
            {
                objProductPageCache.InvalidateModularInfo(productPageId, languageId, countryId, useDate);
                var modularInfo = await Task.FromResult(loadModularInfoFromCmsDatabase(productPageId, countryId, languageId, useDate));
                objProductPageCache.ExplicitSetModularInfo(productPageId, languageId, countryId, useDate, modularInfo, expiration);
            }

            return productPageStore;
        }


        private cmsProductPageDefinition loadProductPageFromCmsDatabase(int productPageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            var dataContext = new databaseDataContext();

            var results = dataContext.ProductPage_Select(productPageId, languageId, countryId, useDate, isTrade);

            var productPageStore = new cmsProductPageDefinition
            {
                objCacheSelect = results.GetResult<productPageCacheSelect>().SingleOrDefault(),
                objUnavailabilityRedirect = results.GetResult<productPageUnavailableRedirect>().SingleOrDefault()
            };

            return productPageStore;
        }


        //public async Task<cmsRealTimeStock> loadRealtimeStockFromCms(Int64 visitorId, Int32? memberId, int productPageId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool displayTradePrice)
        //{
        //    cmsRealTimeStock realtimeStockStore;
        //    realtimeStockStore = await Task.FromResult<cmsRealTimeStock>(loadRealtimeStockFromCmsDatabase(visitorId, memberId, productPageId, languageId, countryId, useDate, memberTradeAccountID, displayTradePrice));
        //    return realtimeStockStore;
        //}


        //private cmsRealTimeStock loadRealtimeStockFromCmsDatabase(Int64 visitorId, Int32? memberId, int productPageId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool displayTradePrice)
        //{


        //    var dataContext = new databaseDataContext();

        //    var  result = dataContext.ProductPage_OrderForm_Select(visitorId, memberId, productPageId, languageId, countryId, useDate, memberTradeAccountID, displayTradePrice, memberTradeAccountID.HasValue);


        //    //  Convert from IEnumerable to a List so a collection query can be run multiple times
        //    _ = new cmsRealTimeStock();

        //    cmsRealTimeStock realtimeStockStore = result.SingleOrDefault();


        //    //realtimeStockStore.productPagePrices = results.GetResult<productPagePrice>().ToList<productPagePrice>();

        //    //var products = results.GetResult<productPageProduct>().ToList();
        //    //realtimeStockStore.productPagePrimarys = products.Where(p => p.PrimaryProduct == true).OrderBy(p => p.SortOrder).ToList();
        //    //realtimeStockStore.productPageSecondarys = products.Where(p => p.PrimaryProduct == false).OrderBy(p => p.SortOrder).ToList();

        //    //realtimeStockStore.alreadyInBasket = results.GetResult<AlreadyInBasket>().ToList<AlreadyInBasket>();
        //    //realtimeStockStore.basketLineTypes = results.GetResult<basketLineType>().ToList<basketLineType>();

        //    return realtimeStockStore;


        //}

        
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

            var techInfo = objProductPageCache.GetTechInfo(productPageId, languageId, countryId, useDate);

            if (techInfo == null)
            {
                techInfo = await Task.FromResult<DatabaseJSONResponse>(loadTechInfoFromCmsDatabase(productPageId, countryId, languageId, useDate));

                objProductPageCache.SetTechInfo(productPageId, languageId, countryId, useDate, techInfo, CacheTimeMinutes);
            }

            return techInfo;
        }

        private DatabaseJSONResponse loadTechInfoFromCmsDatabase(int productPageId, short countryId, byte languageId,  DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            return dataContext.ProductPage_TechInfo_Select(productPageId, countryId, languageId, useDate);
        }




        public async Task<DatabaseJSONResponse> loadModularInfoFromCms(int productPageId, short countryId, byte languageId, DateTime useDate, Int16 CacheTimeMinutes)
        {
            _ = new Models.Database.databaseDataContext();

            ProductPageCacheProvider objProductPageCache = new ProductPageCacheProvider();

            var modularInfo = objProductPageCache.GetModularInfo(productPageId, languageId, countryId, useDate);

            if (modularInfo == null)
            {
                modularInfo = await Task.FromResult<DatabaseJSONResponse>(loadModularInfoFromCmsDatabase(productPageId, countryId, languageId, useDate));

                objProductPageCache.SetModularInfo(productPageId, languageId, countryId, useDate, modularInfo, CacheTimeMinutes);
            }

            return modularInfo;
        }

        private DatabaseJSONResponse loadModularInfoFromCmsDatabase(int productPageId, short countryId, byte languageId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            return dataContext.FetchModularOptions(productPageId, countryId, languageId, useDate);
        }
    }
}