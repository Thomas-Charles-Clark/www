using DocumentFormat.OpenXml.EMMA;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using Tweetinvi.Core.Events;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Cache.ProductPages
{
    public class ProductPageGridCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public PageSectionJSON Get(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);

            var result = (PageSectionJSON)Cache[key];

            return result;
        }

        public void Set(long pageSectionTypeId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate, DatabaseJSONResponse data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(pageSectionTypeId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public void ExplicitSet(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate, DatabaseJSONResponse data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            String key = buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);
            Cache.Remove(key);
        }

        public string InvalidateMultiple(long pageSectionId, byte languageId, short countryId) 
        {
            var keys = Cache.Select(x => x.Key);

            var removalCount = 0;

            foreach (var k in keys) 
            {
                if (k.StartsWith($"PRODUCTPAGEGRID~{pageSectionId}~{languageId}~{countryId}")) 
                {
                    Cache.Remove(k);
                    removalCount++;
                }
            }

            return $"Removed {removalCount} of {keys.Count()} keys from the cache";
        }

        private string buildKey(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            return $"PRODUCTPAGEGRID~{pageSectionId}~{languageId}~{countryId}~{tradeSwitch}~{tradeAccountId}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}
	}
}