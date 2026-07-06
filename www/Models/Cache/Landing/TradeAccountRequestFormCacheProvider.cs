using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.ViewModels.Trade;

namespace wwwDesignersGuild.Models.Cache
{
    public class TradeAccountRequestFormCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public TradeAccountRequestViewModel GetOrCreate(
			short countryId,
            byte languageId,
            DateTime useDate,
			Func<TradeAccountRequestViewModel> factory)
		{
			var key = buildKey(countryId, languageId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (TradeAccountRequestViewModel)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (TradeAccountRequestViewModel)Cache[key];
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

		private string buildKey(short countryId, byte languageId, DateTime useDate)
		{
			return $"TRADE~ACCOUNT~REQUEST~FORM~{countryId}~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}
    }
}