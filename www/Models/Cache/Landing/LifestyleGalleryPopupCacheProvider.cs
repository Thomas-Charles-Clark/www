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
    public class LifestyleGalleryPopupCacheProvider
    {
        public MemoryCache Cache = MvcApplication.CacheSingletons["LifestyleGalleryPopups"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsLifestyleGalleryPopupDefinition GetOrCreate(
			int productPageId,
			byte languageId,
			short countryId,
			DateTime useDate,
			Func<cmsLifestyleGalleryPopupDefinition> factory)
		{
			var key = buildKey(productPageId, languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsLifestyleGalleryPopupDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsLifestyleGalleryPopupDefinition)Cache[key];
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

		private string buildKey(int productPageId, byte languageId, short countryId, DateTime useDate)
		{
			return $"LIFESTYLEGP~{productPageId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

		//public cmsLifestyleGalleryPopupDefinition Get(int productPageId, byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(productPageId, languageId, countryId, useDate);
  //          return (cmsLifestyleGalleryPopupDefinition)Cache[key];
  //      }

  //      public void Set(int productPageId, byte languageId, short countryId, DateTime useDate, cmsLifestyleGalleryPopupDefinition data)
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

  //              var key = buildKey(productPageId, languageId, countryId, useDate);

  //              Cache.Add(new CacheItem(key, data), policy);
  //          }
  //      }


  //      public bool IsSet(int productPageId, byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(productPageId, languageId, countryId, useDate);
  //          return (Cache[key] != null);
  //      }


  //      public void Invalidate(int productPageId, byte languageId, short countryId, DateTime useDate)
  //      {
  //          String key = buildKey(productPageId, languageId, countryId, useDate);
  //          Cache.Remove(key);
  //      }
    }
}