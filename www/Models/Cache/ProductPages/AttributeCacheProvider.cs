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
    public class AttributeCacheProvider : ICacheAttributeProvider
    {

        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsAttributeDefinition Get(byte languageId)
        {
            String key = buildKey(languageId);
            return (cmsAttributeDefinition)Cache[key];
        }


        public void Set(byte languageId, cmsAttributeDefinition data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["productPageCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(languageId);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(byte languageId)
        {
            String key = buildKey(languageId);
            return (Cache[key] != null);
        }


        public void Invalidate(byte languageId)
        {
            String key = buildKey(languageId);
            Cache.Remove(key);
        }


        private string buildKey(byte languageId)
        {
            return String.Format("ATTRIBUTE~{0}", languageId.ToString());
        }
    }
}