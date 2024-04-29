using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;

using Tweetinvi.Parameters;

using U2.Data.ClientTypes;

using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.Models
{
    public class FilterIndexRestrictionCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsFilterIndexRestrictionResult Get(PassFilterIndexRestrictionCacheFetch objFetch)
        {
            return (cmsFilterIndexRestrictionResult)Cache[buildKey(objFetch)];
        }

        public void Set(PassFilterIndexRestrictionCacheFetch objFetch, cmsFilterIndexRestrictionResult data)
        {
            if (Boolean.TryParse(WebConfigurationManager.AppSettings["landingCacheEnabled"], out bool cacheEnabled))
            {
                if (cacheEnabled)
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

                    var key = buildKey(objFetch);

                    Cache.Add(new CacheItem(key, data), policy);
                }
            }
        }

        public bool IsSet(PassFilterIndexRestrictionCacheFetch objFetch)
        {
            return (Cache[buildKey(objFetch)] != null);
        }

        public void Invalidate(PassFilterIndexRestrictionCacheFetch objFetch)
        {
            Cache.Remove(buildKey(objFetch));
        }

        private string buildKey(PassFilterIndexRestrictionCacheFetch objFetch)
        {
            return $"FILTER-INDEX-RESTRICTIONS~{objFetch.Date.Year}~{objFetch.Date.Month}~{objFetch.Date.Day}";
        }
    }
}