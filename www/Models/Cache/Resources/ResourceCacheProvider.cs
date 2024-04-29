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
    public class ResourceCacheProvider : ICacheResourceProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public List<resourceSelect> Get(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (List<resourceSelect>)Cache[key];
        }


        public void Set(byte languageId, short countryId, DateTime useDate, List<resourceSelect> data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["resourceCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["resourceCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(languageId, countryId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            Cache.Remove(key);
        }


        private string buildKey(byte languageId, short countryId, DateTime useDate)
        {
            return String.Format("RESOURCE~{0}~{1}", languageId.ToString(), countryId.ToString());
        }
    }
}