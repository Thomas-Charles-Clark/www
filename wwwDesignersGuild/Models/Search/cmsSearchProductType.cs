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
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.Models.Search
{
    public class cmsSearchProductType : ICmsSearchProductTypeDropDownRepository
    {
        public async Task<cmsSearchProductTypeDropDownDefinition> loadSearchProductTypeDropDownFromCms(short countryId, byte languageId, DateTime useDate)
        {
            // Is list of elements in cache?
            _ = new cmsSearchProductTypeDropDownDefinition();
            SearchProductTypeDropDownCacheProvider objSearchProductTypeCache = new SearchProductTypeDropDownCacheProvider();
            cmsSearchProductTypeDropDownDefinition objSearchProductTypes = objSearchProductTypeCache.Get(countryId, languageId, useDate);

            if (objSearchProductTypes == null)
            {
                objSearchProductTypes = await Task.FromResult<cmsSearchProductTypeDropDownDefinition>(loadSearchProductTypeDropDownFromCmsDatabase(countryId, languageId, useDate));
                objSearchProductTypeCache.Set(objSearchProductTypes, countryId, languageId);
            }

            return objSearchProductTypes;
        }

        private cmsSearchProductTypeDropDownDefinition loadSearchProductTypeDropDownFromCmsDatabase(short countryId, byte languageId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults searchProductTypeResults = dataContext.Search_ProductType_GenerateMultiple(countryId, languageId, useDate);

            cmsSearchProductTypeDropDownDefinition searchProductTypeStore = new cmsSearchProductTypeDropDownDefinition
            {
                ProductTypes = searchProductTypeResults.GetResult<ProductType>().ToList()
            };

            return searchProductTypeStore;
        }
    }
}