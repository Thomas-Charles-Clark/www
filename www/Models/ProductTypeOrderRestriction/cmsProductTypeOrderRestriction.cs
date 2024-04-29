using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models
{
    public class cmsProductTypeOrderRestriction : ICmsProductTypeOrderRestrictionRepository
    {
        public async Task<cmsProductTypeOrderRestrictionDefinition> loadProductTypeOrderRestrictionFromCms(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            cmsProductTypeOrderRestrictionDefinition productTypeOrderRestrictionStore;
            
            ProductTypeOrderRestrictionCacheProvider objProductTypeOrderRestrictionCache = new ProductTypeOrderRestrictionCacheProvider();
            productTypeOrderRestrictionStore = objProductTypeOrderRestrictionCache.Get(memberTradeAccountId, languageId, countryId, useDate);

            if (productTypeOrderRestrictionStore == null)
            {
                productTypeOrderRestrictionStore = await Task.FromResult<cmsProductTypeOrderRestrictionDefinition>(loadProductTypeOrderRestrictionFromCmsDatabase(memberTradeAccountId, languageId, countryId, useDate));
                objProductTypeOrderRestrictionCache.Set(memberTradeAccountId, languageId, countryId, useDate, productTypeOrderRestrictionStore);
            }

            return productTypeOrderRestrictionStore;
        }

        private cmsProductTypeOrderRestrictionDefinition loadProductTypeOrderRestrictionFromCmsDatabase(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults productTypeOrderRestrictionResults = dataContext.ProductType_OrderRestriction_Select(memberTradeAccountId, languageId);

            cmsProductTypeOrderRestrictionDefinition productTypeOrderRestrictionStore = new cmsProductTypeOrderRestrictionDefinition
            {
                ProductTypeOrderRestrictions = productTypeOrderRestrictionResults.GetResult<productTypeOrderRestrictionSelect>().ToList()
            };

            return productTypeOrderRestrictionStore;
        }
    }
}