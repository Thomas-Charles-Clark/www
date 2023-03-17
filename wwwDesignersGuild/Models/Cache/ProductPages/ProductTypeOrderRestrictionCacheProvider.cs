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
    public class ProductTypeOrderRestrictionCacheProvider : ICacheProductTypeOrderRestrictionProvider
    { 
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsProductTypeOrderRestrictionDefinition Get(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(memberTradeAccountId, languageId, countryId, useDate);
            return (cmsProductTypeOrderRestrictionDefinition)Cache[key];
        }

        public void Invalidate(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(memberTradeAccountId, languageId, countryId, useDate);
            Cache.Remove(key);
        }

        public bool IsSet(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(memberTradeAccountId, languageId, countryId, useDate);
            return (Cache[key] != null);
        }

        public void Set(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate, cmsProductTypeOrderRestrictionDefinition data)
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

                var key = buildKey(memberTradeAccountId, languageId, countryId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }
        
        private string buildKey(Int32? memberTradeAccountId, byte languageId, short countryId, DateTime useDate)
        {
            return String.Format("PRODUCT~TYPE~ORDER~RESTRICTION~{0}~{1}~{2}~{3}~{4}~{5}",
                                                                memberTradeAccountId,
                                                                languageId,
                                                                countryId,
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}