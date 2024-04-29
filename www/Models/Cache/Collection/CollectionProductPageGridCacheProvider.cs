using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Cache.Collection
{
    public class CollectionProductPageGridCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public PageSectionJSON Get(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(collectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            return (PageSectionJSON)Cache[key];
        }

        public void Set(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate, DatabaseJSONResponse data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["collectionProductPageGridCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["collectionProductPageGridCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(collectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public void ExplicitSet(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate, DatabaseJSONResponse data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildKey(collectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(collectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(collectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            Cache.Remove(key);
        }


        private string buildKey(short collectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            return $"COLLECTIONPRODUCTPAGEGRID~{collectionId}~{languageId}~{countryId}~{tradeSwitch}~{tradeAccountId}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }
}