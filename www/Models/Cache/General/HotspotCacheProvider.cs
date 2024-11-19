using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.Models.Cache.General
{
    public class HotspotCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsHotspotDefinition Get(PassHotspotCacheFetch objFetch)
        {
            return (cmsHotspotDefinition)Cache[buildKey(objFetch)];
        }

        public void Set(PassHotspotCacheFetch objFetch, cmsHotspotDefinition data)
        {
            // This is a stupid: it only disables hotspots if hotspotCacheEnabled <> true / false.
            // Silly.
            if (Boolean.TryParse(WebConfigurationManager.AppSettings["hotspotCacheEnabled"], out bool _))
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

                var key = buildKey(objFetch);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public bool IsSet(PassHotspotCacheFetch objFetch)
        {
            return (Cache[buildKey(objFetch)] != null);
        }

        public void Invalidate(PassHotspotCacheFetch objFetch)
        {
            Cache.Remove(buildKey(objFetch));
        }

        private string buildKey(PassHotspotCacheFetch objFetch)
        {
            return $"HOTSPOTS~{objFetch}";
        }
    }
}