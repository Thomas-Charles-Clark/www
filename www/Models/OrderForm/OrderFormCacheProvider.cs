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


namespace wwwDesignersGuild.Models.OrderForm
{
    public class OrderFormCacheProvider
    {
        public MemoryCache Cache = MvcApplication.CacheSingletons["OrderForms"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsRealTimeStock GetOrCreate(
			int productPageId,
            byte languageId,
            short countryId,
            int? tradeAccountId,
            bool displayTradePrice,
            bool isLifestyle,
            bool countryMatch,
            DateTime useDate,
			Func<cmsRealTimeStock> factory)
		{
			var key = buildKey(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (cmsRealTimeStock)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsRealTimeStock)Cache[key];
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

		private string buildKey(int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
		{
			return $"ORDERFORMCACHE~{productPageId}~{languageId}~{countryId}~{tradeAccountId}~{displayTradePrice}~{isLifestyle}~{countryMatch}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}
    }
}