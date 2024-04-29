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
    public class SearchProductTypeDropDownCacheProvider : ICacheSearchProductTypeDropDownProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }
        
        public cmsSearchProductTypeDropDownDefinition Get(short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(countryId, languageId);
            return (cmsSearchProductTypeDropDownDefinition)Cache[key];
        }
        
        public void Set(cmsSearchProductTypeDropDownDefinition data, short countryId, byte languageId)
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

                var key = buildKey(countryId, languageId);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }
        
        public bool IsSet(short countryId, byte languageId)
        {
            String key = buildKey(countryId, languageId);
            return (Cache[key] != null);
        }


        public void Invalidate(short countryId, byte languageId)
        {
            String key = buildKey(countryId, languageId);
            Cache.Remove(key);
        }


        private string buildKey(short countryId, byte languageId)
        {
            return String.Format(String.Format("SEARCHPRODUCTTYPEDROPDOWN~{0}~{1}", countryId, languageId));
        }
    }
}