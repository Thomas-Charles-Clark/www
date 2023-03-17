using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Cache.General
{
    public class ScrapbookCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsScrapbookImagesDefinition Get(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate)
        {
            return (cmsScrapbookImagesDefinition)Cache[buildKey(visitorId, memberId, countryId, languageId, useDate)];
        }

        public void Set(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate, cmsScrapbookImagesDefinition data)
        {
            if (Boolean.TryParse(WebConfigurationManager.AppSettings["scrapbookCacheEnabled"], out bool _))
            {
                var policy = new CacheItemPolicy();

                if (Int16.TryParse(WebConfigurationManager.AppSettings["scrapbookCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(visitorId, memberId, countryId, languageId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public bool IsSet(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(visitorId, memberId, countryId, languageId, useDate);
            return (Cache[key] != null);
        }

        public void Invalidate(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(visitorId, memberId, countryId, languageId, useDate);
            Cache.Remove(key);
        }

        private string buildKey(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate)
        {
            return $"SCRAPBOOK~{visitorId}~{memberId}~{countryId}~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }
}