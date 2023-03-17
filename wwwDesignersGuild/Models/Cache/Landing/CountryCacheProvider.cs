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


    public class CountryCacheProvider : ICacheCountryProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsCountryDefinition Get()
        {
            String key = buildKey();
            return (cmsCountryDefinition)Cache[key];
        }


        public void Set(cmsCountryDefinition data)
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

                var key = buildKey();

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet()
        {
            String key = buildKey();
            return (Cache[key] != null);
        }


        public void Invalidate()
        {
            String key = buildKey();
            Cache.Remove(key);
        }


        private string buildKey()
        {
            return String.Format("COUNTRY");
        }


    }


}