using DocumentFormat.OpenXml.EMMA;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using Tweetinvi.Core.Events;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Cache.ProductPages
{
    public class ProductPageGridPageSectionCacheProvider
	{
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public ProductPageGridPageSection GetOrCreate(
			long pageSectionId,
			byte languageId,
			short countryId,
			int? tradeAccountId,
			bool countryMatch,
			DateTime useDate,
			bool shouldLock,
			bool isSearch,
			NameValueCollection queryString,
			Func<NameValueCollection, ProductPageGridPageSection> factory)
		{
			var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "Get");

			// Fast path: Try to get existing cache without locking
			var data = (ProductPageGridPageSection)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			// can't lock twice in a row: see GeneralAPIController, FetchProductPageGridDefinition
			if (shouldLock)
			{
				lock (_syncLock)
				{
					// Necessary double-check pattern: Has another thread built it while we waited?
					data = (ProductPageGridPageSection)Cache[key];
					if (data != null)
						return data;

					// Execute the creation block passed by the caller
					data = factory(queryString);

					// Cache it if caching is enabled
					if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheEnabled"], out bool enabled) && enabled)
					{
						var policy = new CacheItemPolicy();

						if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheDuration"], out short cacheDurationMinutes))
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
			}
			else
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (ProductPageGridPageSection)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory(queryString);

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheDuration"], out short cacheDurationMinutes))
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

		private string buildKey(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate, bool isSearch, string from)
		{
			var tradeSwitch = tradeAccountId.HasValue;

			var key = $"PRODUCTPAGEGRID~PAGESECTION~{pageSectionId}~{languageId}~{countryId}~{tradeSwitch}~{tradeAccountId}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";

#if DEBUG
			if (pageSectionId == 0)
			{
				throw new Exception($"ProductPageGridPageSectionCacheProvider.buildKey built \"{key}\"");
			}
#endif

			return key;
		}

		public ProductPageGridPageSection Get(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool isSearch, DateTime useDate)
		{
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "Get");

			var result = Cache[key];

            return (ProductPageGridPageSection)result;
		}

		public void Set(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool isSearch, DateTime useDate, ProductPageGridPageSection data)
		{
            Boolean.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageGridCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "Set");

                Cache.Add(new CacheItem(key, data), policy);
			}
		}

		public void ExplicitSet(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool isSearch, DateTime useDate, ProductPageGridPageSection data, DateTimeOffset expiration)
		{
			CacheItemPolicy policy = new CacheItemPolicy
			{
				AbsoluteExpiration = expiration
			};

			var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "ExplicitSet");

			Cache.Add(new CacheItem(key, data), policy);
		}

		public bool IsSet(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool isSearch, DateTime useDate)
        {
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "IsSet");
            return (Cache[key] != null);
        }


        public void Invalidate(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, bool isSearch, DateTime useDate)
        {
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, isSearch, "Invalidate");
            Cache.Remove(key);
        }

        public string InvalidateMultiple(long pageSectionId, byte languageId, short countryId) 
        {
            var keys = Cache.Select(x => x.Key);

            var removalCount = 0;

            foreach (var k in keys) 
            {
                if (k.StartsWith($"PRODUCTPAGEGRID~PAGESECTION~{pageSectionId}~{languageId}~{countryId}")) 
                {
                    Cache.Remove(k);
                    removalCount++;
                }
            }

            return $"Removed {removalCount} of {keys.Count()} keys from the cache";
        }
	}
}