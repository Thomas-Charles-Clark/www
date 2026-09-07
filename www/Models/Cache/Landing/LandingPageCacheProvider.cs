using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;
using wwwDesignersGuild.Models.EmailCapturePopupForm;
using DocumentFormat.OpenXml.Wordprocessing;

namespace wwwDesignersGuild.Models.Cache
{
    public class LandingPageCacheProvider
    {
        public MemoryCache Cache = MvcApplication.CacheSingletons["LandingPages"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsLandingPageDefinition GetOrCreate(
			int pageId,
            byte languageId,
            short countryId,
            DateTime useDate,
            bool isTrade,
			HttpResponseBase responseBase,
			Func<cmsLandingPageDefinition> factory)
		{
			var key = buildKey(pageId, languageId, countryId, useDate, isTrade);

			// Fast path: Try to get existing cache without locking
			var data = (cmsLandingPageDefinition)Cache[key];

			if (data != null)
			{
				if (responseBase != null)
				{
					responseBase.AddHeader("DG-Cache-Hit", "true");
				}

				return data;
			}

			responseBase?.AddHeader("DG-Cache-Hit", "false");

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsLandingPageDefinition)Cache[key];
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

		private string buildKey(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
		{
			return $"PAGE~{pageId}~{languageId}~{countryId}~{isTrade}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

        public void Invalidate(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            lock (_syncLock)
			{
				Cache.Remove(buildKey(pageId, languageId, countryId, useDate, isTrade));
			}
        }
    }
}