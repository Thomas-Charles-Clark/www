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
    public class BannerCacheProvider
	{
        public MemoryCache Cache = MvcApplication.CacheSingletons["Banners"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsBannerDefinition GetOrCreate(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch, Func<cmsBannerDefinition> factory)
		{
			var key = buildKey(countryId, languageId, useDate, isTrade, platformSwitch);

			// Fast path: Try to get existing cache without locking
			var data = (cmsBannerDefinition)Cache[key];

			if (data != null) 
                return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsBannerDefinition)Cache[key];
				if (data != null) 
                    return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["bannerCacheEnabled"], out bool enabled) && enabled)
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

					Cache.Set(key, data, policy); // Use Set instead of Add to safely overwrite/insert
				}
			}

			return data;
		}
        
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