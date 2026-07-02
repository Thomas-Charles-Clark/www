using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Cache
{
    public class ResourceCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsResourceDefinition GetOrCreate(
			byte languageId,
            short countryId, 
            DateTime useDate,
			Func<cmsResourceDefinition> factory)
		{
			var key = buildKey(languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsResourceDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsResourceDefinition)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["resourceCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["resourceCacheDuration"], out short cacheDurationMinutes))
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

		private string buildKey(byte languageId, short countryId, DateTime useDate)
		{
			return $"RESOURCE~{languageId}~{countryId}";
		}

		public void Invalidate(byte languageId, short countryId, DateTime useDate)
        {
			lock (_syncLock)
			{
				Cache.Remove(buildKey(languageId, countryId, useDate));
			}
        }
    }
}