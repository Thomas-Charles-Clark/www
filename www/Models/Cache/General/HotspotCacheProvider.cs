using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.Models.Cache.General
{
    public class HotspotCacheProvider
	{
		private ObjectCache Cache { get { return MemoryCache.Default; } }

		// Static lock ensures all instances of this provider in the same w3wp.exe share it
		private static readonly object _syncLock = new object();

		public cmsHotspotDefinition GetOrCreate(
			PassHotspotCacheFetch objFetch,
			Func<cmsHotspotDefinition> factory)
		{
			var key = buildKey(objFetch);

			// Fast path: Try to get existing cache without locking
			var data = (cmsHotspotDefinition)Cache[key];

			if (data != null)
				return data;

			// Slow path: Lock to prevent multiple threads from generating the same key
			lock (_syncLock)
			{
				// Necessary double-check pattern: Has another thread built it while we waited?
				data = (cmsHotspotDefinition)Cache[key];
				if (data != null)
					return data;

				// Execute the creation block passed by the caller
				data = factory();

				// Cache it if caching is enabled
				if (data != null && Boolean.TryParse(WebConfigurationManager.AppSettings["hotspotCacheEnabled"], out bool enabled) && enabled)
				{
					var policy = new CacheItemPolicy();

					if (Int16.TryParse(WebConfigurationManager.AppSettings["hotspotCacheDuration"], out short cacheDurationMinutes))
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

		private string buildKey(PassHotspotCacheFetch objFetch)
		{
			// objFetch overrides ToString with Newtonsoft.Json
			return $"HOTSPOTS~{objFetch}";
		}

		//public cmsHotspotDefinition Get(PassHotspotCacheFetch objFetch)
		//      {
		//          return (cmsHotspotDefinition)Cache[buildKey(objFetch)];
		//      }

		//      public void Set(PassHotspotCacheFetch objFetch, cmsHotspotDefinition data)
		//      {
		//          // This is a stupid: it only disables hotspots if hotspotCacheEnabled <> true / false.
		//          // Silly.
		//          if (Boolean.TryParse(WebConfigurationManager.AppSettings["hotspotCacheEnabled"], out bool _))
		//          {
		//              var policy = new CacheItemPolicy();

		//              if (Int16.TryParse(WebConfigurationManager.AppSettings["hotspotCacheDuration"], out short cacheDurationMinutes))
		//              {
		//                  policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
		//              }
		//              else
		//              {
		//                  policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
		//              }

		//              var key = buildKey(objFetch);

		//              Cache.Add(new CacheItem(key, data), policy);
		//          }
		//      }

		//      public bool IsSet(PassHotspotCacheFetch objFetch)
		//      {
		//          return (Cache[buildKey(objFetch)] != null);
		//      }

		//      public void Invalidate(PassHotspotCacheFetch objFetch)
		//      {
		//          Cache.Remove(buildKey(objFetch));
		//      }
	}
}