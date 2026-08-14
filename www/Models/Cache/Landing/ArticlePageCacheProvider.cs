using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.Models.Cache
{
    public class ArticlePageCacheProvider
    {
		public MemoryCache Cache = MvcApplication.CacheSingletons["Articles"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsArticlePageDefinition GetOrCreate(
			int articleId,
            byte languageId,
            short countryId,
            DateTime useDate,
			Func<cmsArticlePageDefinition> factory)
		{
			var key = buildKey(articleId, languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsArticlePageDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsArticlePageDefinition)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["articleCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["articleCacheDuration"], out short cacheDurationMinutes))
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

        public void Invalidate(int articleId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(articleId, languageId, countryId, useDate);

			lock (_syncLock)
			{
				Cache.Remove(key);
			}
        }

        private string buildKey(int articleId, byte languageId, short countryId, DateTime useDate)
        {
            return String.Format($"ARTICLE~{articleId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}");
        }
    }
}