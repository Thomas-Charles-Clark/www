using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;


namespace wwwDesignersGuild.Models.OrderForm
{
    public class OrderFormCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsRealTimeStock Get(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);
            return (cmsRealTimeStock)Cache[key];
        }

        public void Set(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate, cmsRealTimeStock data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["orderFormCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["orderFormCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public void ExplicitSet(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate, serverSearchDatabase data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);
            Cache.Remove(key);
        }


        private string buildKey(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {    
            return $"ORDERFORMCACHE~{productPageId}~{languageId}~{countryId}~{tradeAccountId}~{displayTradePrice}~{isLifestyle}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }


    public class cmsOrderForm
    {
        public async Task<cmsRealTimeStock> loadOrderFormFromCms(long visitorId, int? memberId, int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {
            //  Is result already in cache?
            cmsRealTimeStock result = null;

            var cache = new OrderFormCacheProvider();

            result = cache.Get(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);

            if (result == null)
            {
                result = await Task.FromResult<cmsRealTimeStock>(loadOrderFormFromDatabase(visitorId, memberId, productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate));

                cache.Set(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate, result);
            }

            return result;
        }

        private cmsRealTimeStock loadOrderFormFromDatabase(long visitorId, int? memberId, int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.ProductPage_OrderForm_Select(visitorId, memberId, productPageId, languageId, countryId, useDate, tradeAccountId, displayTradePrice, tradeAccountId.HasValue, isLifestyle, countryMatch);

            var realtimeStockStore = result.SingleOrDefault();

            return realtimeStockStore;
        }
    }
}