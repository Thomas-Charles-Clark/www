using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Database;
using System.Web.Configuration;
using System.Diagnostics;

namespace wwwDesignersGuild.Models.Cache
{
    public class ProductPageCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsProductPageDefinition GetOrCreate(
			int productPageId,
            byte languageId,
            short countryId,
            bool tradeSwitch,
            bool? platformSwitch,
            DateTime useDate,
			HttpResponseBase responseBase,
			Func<cmsProductPageDefinition> factory)
		{
			var key = buildKey(productPageId, languageId, countryId, tradeSwitch, platformSwitch, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsProductPageDefinition)Cache[key];

			if (data != null)
			{
				responseBase.AddHeader("DG-Cache-Hit", "true");

				return data;
			}

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsProductPageDefinition)Cache[key];

				responseBase.AddHeader("DG-Cache-Hit", "false");

				if (data != null) 
				{
					return data;
				}

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

        private string buildKey(int productPageId, byte languageId, short countryId, bool tradeSwitch, bool? platformSwitch, DateTime useDate)
        {
			var key = $"PRODUCTPAGE~{productPageId}~{languageId}~{countryId}~{tradeSwitch}~{platformSwitch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";

#if DEBUG
			System.Diagnostics.Debug.WriteLine($"ProductPageCacheProvider.buildKey {key}");
#endif

			return key;
        }

        public DatabaseJSONResponse GetOrCreateTechInfo(
			int productPageId,
            byte languageId,
            short countryId,
            DateTime useDate,
            Func<DatabaseJSONResponse> factory
			)
		{
			var key = buildTechInfoKey(productPageId, languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (DatabaseJSONResponse)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (DatabaseJSONResponse)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["menuCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["menuCacheDuration"], out short cacheDurationMinutes))
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

		private string buildTechInfoKey(int productPageId, byte languageId, short countryId, DateTime useDate)
		{
			return $"TECHINFO~{productPageId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

        public DatabaseJSONResponse GetOrCreateModularInfo(
			int productPageId,
			byte languageId,
			short countryId,
			DateTime useDate,
			Func<DatabaseJSONResponse> factory)
        {
			var key = buildModularInfoKey(productPageId, languageId, countryId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (DatabaseJSONResponse)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (DatabaseJSONResponse)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["menuCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["menuCacheDuration"], out short cacheDurationMinutes))
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

		private string buildModularInfoKey(int productPageId, byte languageId, short countryId, DateTime useDate)
		{
			return $"MODULARDATA~{productPageId}~{languageId}~{countryId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

		public void Invalidate(int productPageId, byte languageId, short countryId, bool tradeSwitch, bool? platformSwitch, DateTime useDate)
		{
			lock (_syncLock)
			{
				Cache.Remove(buildKey(productPageId, languageId, countryId, tradeSwitch, true, useDate));
				Cache.Remove(buildKey(productPageId, languageId, countryId, tradeSwitch, false, useDate));
				Cache.Remove(buildKey(productPageId, languageId, countryId, tradeSwitch, null, useDate));
				Cache.Remove(buildTechInfoKey(productPageId, languageId, countryId, useDate));
				Cache.Remove(buildModularInfoKey(productPageId, languageId, countryId, useDate));
			}
		}
	}
}