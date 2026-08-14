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
using wwwDesignersGuild.Models.Redirect;

namespace wwwDesignersGuild.Models.Search
{
    public class SearchCacheProvider
    {
        private MemoryCache Cache = MvcApplication.CacheSingletons["Searches"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public serverSearchDatabase GetOrCreate(
			byte languageId,
            short countryId,
            string searchTerm,
            int? tradeAccountId,
            DateTime useDate,
			Func<serverSearchDatabase> factory)
		{
			var key = buildKey(languageId, countryId, searchTerm, tradeAccountId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (serverSearchDatabase)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (serverSearchDatabase)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["searchCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["searchCacheDuration"], out short cacheDurationMinutes))
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

		private string buildKey(byte languageId, short countryId, string searchTerm, int? tradeAccountId, DateTime useDate)
		{
			return $"SEARCHCACHE~{languageId}~{countryId}~{searchTerm}~{tradeAccountId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

		public void Invalidate(byte languageId, short countryId, string searchTerm, int? tradeAccountId, DateTime useDate)
        {
			lock (_syncLock)
			{
				Cache.Remove(buildKey(languageId, countryId, searchTerm, tradeAccountId, useDate));
			}
        }
    }
}