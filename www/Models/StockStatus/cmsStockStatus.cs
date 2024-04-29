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
    public class cmsStockStatus : ICmsStockStatusRepository
    {
        public async Task<cmsStockStatusDefinition> loadStockStatusesFromCms(byte languageId, short countryId, DateTime useDate)
        {
            cmsStockStatusDefinition stockStatusStore;

            StockStatusCacheProvider objStockStatusCache = new StockStatusCacheProvider();
            stockStatusStore = objStockStatusCache.Get(languageId, countryId, useDate);

            if (stockStatusStore == null)
            {
                stockStatusStore = await Task.FromResult<cmsStockStatusDefinition>(loadStockStatusFromCmsDatabase(languageId, countryId, useDate));
                objStockStatusCache.Set(languageId, countryId, useDate, stockStatusStore);
            }

            return stockStatusStore;
        }

        private cmsStockStatusDefinition loadStockStatusFromCmsDatabase(byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults stockStatusResults = dataContext.StockStatus_Select(languageId);

            cmsStockStatusDefinition stockStatusStore = new cmsStockStatusDefinition
            {
                StockStatuses = stockStatusResults.GetResult<StockStatusSelect>().ToList()
            };

            return stockStatusStore;
        }
    }
}