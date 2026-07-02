using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Cache.ProductPages
{
    public class ProductPageGridCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public PageSectionJSON GetOrCreate(
			long pageSectionId,
            byte languageId,
            short countryId,
            bool tradeSwitch,
            int? tradeAccountId,
            bool countryMatch,
            DateTime useDate,
			Func<PageSectionJSON> factory)
		{
			var key = buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (PageSectionJSON)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (PageSectionJSON)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["productPageCacheEnabled"], out bool enabled) && enabled)
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

					Cache.Set(key, data, policy); // Use Set instead of Add to safely overwrite/insert
				}
			}

			return data;
		}

		private string buildKey(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
		{
			return $"PRODUCTPAGEGRID~{pageSectionId}~{languageId}~{countryId}~{tradeSwitch}~{tradeAccountId}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

        public void Invalidate(long pageSectionId, byte languageId, short countryId, bool tradeSwitch, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
			lock (_syncLock)
			{
				Cache.Remove(buildKey(pageSectionId, languageId, countryId, tradeSwitch, tradeAccountId, countryMatch, useDate));
			}
        }

        public string InvalidateMultiple(long pageSectionId, byte languageId, short countryId) 
        {
			lock (_syncLock)
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
        }
    }
}