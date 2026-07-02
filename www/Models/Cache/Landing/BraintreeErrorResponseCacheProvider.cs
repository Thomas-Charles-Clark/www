using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Cache
{
    public class BraintreeErrorResponseCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public IEnumerable<braintreeErrorResponseSelect> GetOrCreate(
			byte languageId,
			DateTime useDate,
			Func<IEnumerable<braintreeErrorResponseSelect>> factory)
		{
			var key = buildKey(languageId, useDate);

			// Fast path: Try to get existing cache without locking
			var data = (IEnumerable<braintreeErrorResponseSelect>)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (IEnumerable<braintreeErrorResponseSelect>)Cache[key];
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

		private string buildKey(byte languageId, DateTime useDate)
		{
			return $"BRAINTREE-ERROR-RESPONSES~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
		}

		//public List<braintreeErrorResponseSelect> Get(byte languageId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, useDate);
  //          return (List<braintreeErrorResponseSelect>)Cache[key];
  //      }

  //      public void Set(byte languageId, DateTime useDate, List<braintreeErrorResponseSelect> data)
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

  //              var key = buildKey(languageId, useDate);

  //              Cache.Add(new CacheItem(key, data), policy);
  //          }
  //      }


  //      public bool IsSet(byte languageId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, useDate);
  //          return (Cache[key] != null);
  //      }


  //      public void Invalidate(byte languageId, DateTime useDate)
  //      {
  //          String key = buildKey(languageId, useDate);
  //          Cache.Remove(key);
  //      }
    }
}