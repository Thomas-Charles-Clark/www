using DocumentFormat.OpenXml.EMMA;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using Tweetinvi.Core.Events;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Cache.ProductPages
{
    public class ProductPageGridPageSectionCacheProvider
	{
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		public ProductPageGridPageSection Get(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate)
		{
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, "Get");

			var result = Cache[key];

            return (ProductPageGridPageSection)result;
		}

		public void Set(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate, ProductPageGridPageSection data)
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

                var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, "Set");

                Cache.Add(new CacheItem(key, data), policy);
            }
		}

		public void ExplicitSet(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate, ProductPageGridPageSection data, DateTimeOffset expiration)
		{
			CacheItemPolicy policy = new CacheItemPolicy
			{
				AbsoluteExpiration = expiration
			};

			var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, "ExplicitSet");

			Cache.Add(new CacheItem(key, data), policy);
		}

		public bool IsSet(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, "IsSet");
            return (Cache[key] != null);
        }


        public void Invalidate(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate)
        {
            var key = buildKey(pageSectionId, languageId, countryId, tradeAccountId, countryMatch, useDate, "Invalidate");
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

        private string buildKey(long pageSectionId, byte languageId, short countryId, int? tradeAccountId, bool countryMatch, DateTime useDate, string from)
        {
            var tradeSwitch = tradeAccountId.HasValue;

            var key = $"PRODUCTPAGEGRID~PAGESECTION~{pageSectionId}~{languageId}~{countryId}~{tradeSwitch}~{tradeAccountId}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";

            System.Diagnostics.Debug.WriteLine("");
			System.Diagnostics.Debug.WriteLine($"ProductPageGridPageSectionCacheProvider called through {from} built key \"{key}\" ");
			System.Diagnostics.Debug.WriteLine("");

			return key;
		}
	}
}