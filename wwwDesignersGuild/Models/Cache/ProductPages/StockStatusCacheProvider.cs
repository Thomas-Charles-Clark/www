using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Cache
{
    public class StockStatusCacheProvider : ICacheStockStatusProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsStockStatusDefinition Get(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (cmsStockStatusDefinition)Cache[key];
        }

        public void Invalidate(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            Cache.Remove(key);
        }

        public bool IsSet(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (Cache[key] != null);
        }

        public void Set(byte languageId, short countryId, DateTime useDate, cmsStockStatusDefinition data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["productPageCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(languageId, countryId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        private string buildKey(byte languageId, short countryId, DateTime useDate)
        {
            return String.Format("STOCK~STATUS~{0}~{1}~{2}~{3}~{4}",
                                                                languageId.ToString(),
                                                                countryId.ToString(),
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}