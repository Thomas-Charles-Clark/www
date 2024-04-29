using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Cache
{
    public class BannerCacheProvider : ICacheBannerProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsBannerDefinition Get(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            var key = buildKey(countryId, languageId, useDate, isTrade, platformSwitch);
            return (cmsBannerDefinition)Cache[key];
        }

        public void Set(cmsBannerDefinition data, short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["bannerCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["bannerCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(countryId, languageId, useDate, isTrade, platformSwitch);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public bool IsSet(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            var key = buildKey(countryId, languageId, useDate, isTrade, platformSwitch);
            return (Cache[key] != null);
        }

        public void Invalidate(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            var key = buildKey(countryId, languageId, useDate, isTrade, platformSwitch);
            Cache.Remove(key);
        }

        private string buildKey(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            return $"BANNERS~{countryId}~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}~{isTrade}~{platformSwitch}";
        }
    }
}