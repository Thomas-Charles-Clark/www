using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Cache.General
{
    public class DotDigitalTrackingBehaviourCacheProvider
	{
		public MemoryCache Cache = MvcApplication.CacheSingletons["DotDigitalTrackingBehaviours"];

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public DotDigitalTrackingBehaviourProfile GetOrCreate(
			short countryId,
			byte languageId,
			DateTime useDate,
			Func<DotDigitalTrackingBehaviourProfile> factory)
		{
			var key = buildKey(countryId, languageId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (DotDigitalTrackingBehaviourProfile)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (DotDigitalTrackingBehaviourProfile)Cache[key];

				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheDuration"], out short cacheDurationMinutes))
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

		//public void Set(short countryId, byte languageId, DateTime useDate, DotDigitalTrackingBehaviourProfile data)
  //      {
  //          if (Boolean.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheEnabled"], out bool _))
  //          {
  //              var policy = new CacheItemPolicy();

  //              if (Int16.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheDuration"], out short cacheDurationMinutes))
  //              {
  //                  policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
  //              }
  //              else
  //              {
  //                  policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
  //              }

  //              var key = buildKey(countryId, languageId, useDate);

  //              Cache.Add(new CacheItem(key, data), policy);
  //          }
  //      }

  //      public bool IsSet(short countryId, byte languageId, DateTime useDate)
  //      {
  //          String key = buildKey(countryId, languageId, useDate);
  //          return (Cache[key] != null);
  //      }

  //      public void Invalidate(short countryId, byte languageId, DateTime useDate)
  //      {
  //          String key = buildKey(countryId, languageId, useDate);
  //          Cache.Remove(key);
  //      }

        private string buildKey(short countryId, byte languageId, DateTime useDate)
        {
            return $"DOT-DIGITAL-BEHAVIOUR-PROFILE~{countryId}~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }
}