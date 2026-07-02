using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Cache
{
    public class ArticleTypeCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsArticleTypeDefinition GetOrCreate(
			byte languageId,
            short countryId,
            DateTime useDate,
			Func<cmsArticleTypeDefinition> factory)
		{
			var key = buildKey(languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsArticleTypeDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsArticleTypeDefinition)Cache[key];
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

		private string buildKey(byte languageId, short countryId, DateTime useDate)
		{
			return $"ARTICLE~TYPE~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

		//public cmsArticleTypeDefinition Get(byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, countryId, useDate);
  //          return (cmsArticleTypeDefinition)Cache[key];
  //      }


  //      public void Set(byte languageId, short countryId, DateTime useDate, cmsArticleTypeDefinition data)
  //      {
  //          Boolean.TryParse(WebConfigurationManager.AppSettings["landingCacheEnabled"], out bool cacheEnabled);

  //          if (cacheEnabled)
  //          {
  //              var policy = new CacheItemPolicy();
  //              if (Int16.TryParse(WebConfigurationManager.AppSettings["landingCacheDuration"], out short cacheDurationMinutes))
  //              {
  //                  policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
  //              }
  //              else
  //              {
  //                  policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
  //              }

  //              var key = buildKey(languageId, countryId, useDate);

  //              Cache.Add(new CacheItem(key, data), policy);
  //          }
  //      }


  //      public bool IsSet(byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, countryId, useDate);
  //          return (Cache[key] != null);
  //      }


  //      public void Invalidate(byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, countryId, useDate);
  //          Cache.Remove(key);
  //      }


  //      private string buildKey(byte languageId, short countryId, DateTime useDate)
  //      {
  //          return String.Format("ARTICLE~TYPE~{0}~{1}~{2}~{3}~{4}", 
  //                                                              languageId.ToString(),
  //                                                              countryId.ToString(),
  //                                                              useDate.Year.ToString(),
  //                                                              useDate.Month.ToString(),
  //                                                              useDate.Day.ToString());
  //      }
    }
}