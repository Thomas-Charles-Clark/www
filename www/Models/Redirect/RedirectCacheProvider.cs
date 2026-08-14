using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Redirect
{
	public class RedirectCacheProvider
	{
		public MemoryCache Cache = MvcApplication.CacheSingletons["Redirects"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public RedirectDefinition GetOrCreate(
			DateTime useDate,
			Func<RedirectDefinition> factory)
		{
			var key = buildKey(useDate);

			// Fast path: Try to get existing cache without locking
			var data = (RedirectDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (RedirectDefinition)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["landingCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["landingCacheDuration"], out short cacheDurationMinutes))
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

		private string buildKey(DateTime useDate)
		{
			return $"REDIRECTS~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}
	}
}