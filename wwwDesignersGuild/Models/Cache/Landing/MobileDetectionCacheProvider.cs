using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Cache
{
    public class MobileDetectionCacheProvider : ICacheMobileDetectionProvider
    {

        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public MobileDetectionDefinition Get(String userAgent)
        {
            String key = userAgent;
            return (MobileDetectionDefinition)Cache[key];
        }


        public void Set(String userAgent, MobileDetectionDefinition data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["landingCacheEnabled"], out bool cacheEnabled);

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

                Cache.Add(new CacheItem(userAgent, data), policy);
            }
        }


        public bool IsSet(String userAgent)
        {
            String key = userAgent;
            return (Cache[key] != null);
        }


        public void Invalidate(String userAgent)
        {
            String key = userAgent;
            Cache.Remove(key);
        }

    }

}