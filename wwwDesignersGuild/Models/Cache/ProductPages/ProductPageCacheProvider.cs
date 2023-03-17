using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Database;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Cache
{
    public class ProductPageCacheProvider : ICacheProductPageProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsProductPageDefinition Get(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeSwitch, useDate);
            return (cmsProductPageDefinition)Cache[key];
        }

        public void Set(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate, cmsProductPageDefinition data)
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

                var key = buildKey(productPageId, languageId, countryId, tradeSwitch, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public void ExplicitSet(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate, cmsProductPageDefinition data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildKey(productPageId, languageId, countryId, tradeSwitch, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeSwitch, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, tradeSwitch, useDate);
            Cache.Remove(key);
        }


        private string buildKey(int productPageId, byte languageId, short countryId, bool tradeSwitch, DateTime useDate)
        {
            return $"PRODUCTPAGE~{productPageId}~{languageId}~{countryId}~{tradeSwitch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }



        public DatabaseJSONResponse GetTechInfo(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildTechInfoKey(productPageId, languageId, countryId, useDate);
            return (DatabaseJSONResponse)Cache[key];
        }

        public void SetTechInfo(int productPageId, byte languageId, short countryId, DateTime useDate, DatabaseJSONResponse data, Int16 CacheTimeMinutes)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(CacheTimeMinutes)
            };

            String key = buildTechInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public void ExplicitSetTechInfo(int productPageId, byte languageId, short countryId, DateTime useDate, DatabaseJSONResponse data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildTechInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsTechInfoSet(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildTechInfoKey(productPageId, languageId, countryId, useDate);
            return (Cache[key] != null);
        }

        public void InvalidateTechInfo(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildTechInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Remove(key);
        }

        private string buildTechInfoKey(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            return $"TECHINFO~{productPageId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }



        public DatabaseJSONResponse GetModularInfo(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildModularInfoKey(productPageId, languageId, countryId, useDate);
            return (DatabaseJSONResponse)Cache[key];
        }

        public void SetModularInfo(int productPageId, byte languageId, short countryId, DateTime useDate, DatabaseJSONResponse data, Int16 CacheTimeMinutes)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(CacheTimeMinutes)
            };

            String key = buildModularInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public void ExplicitSetModularInfo(int productPageId, byte languageId, short countryId, DateTime useDate, DatabaseJSONResponse data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildModularInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsModularInfoSet(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildModularInfoKey(productPageId, languageId, countryId, useDate);
            return (Cache[key] != null);
        }

        public void InvalidateModularInfo(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildModularInfoKey(productPageId, languageId, countryId, useDate);
            Cache.Remove(key);
        }

        private string buildModularInfoKey(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            return $"MODULARDATA~{productPageId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }
}