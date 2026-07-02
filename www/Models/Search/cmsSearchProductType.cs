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
            var objSearchProductTypeCache = new SearchProductTypeDropDownCacheProvider();

            var objSearchProductTypes = objSearchProductTypeCache.GetOrCreate(countryId, languageId, useDate, () => {
                
                var result = Task.Run(() => loadSearchProductTypeDropDownFromCmsDatabase(countryId, languageId, useDate)).Result;
                
                return result;

            });

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