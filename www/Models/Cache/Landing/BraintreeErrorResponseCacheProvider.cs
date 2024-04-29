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
    public class BraintreeErrorResponseCacheProvider : ICacheBraintreeErrorResponseProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public List<braintreeErrorResponseSelect> Get(byte languageId, DateTime useDate)
        {
            String key = buildKey(languageId, useDate);
            return (List<braintreeErrorResponseSelect>)Cache[key];
        }


        public void Set(byte languageId, DateTime useDate, List<braintreeErrorResponseSelect> data)
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

                var key = buildKey(languageId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(byte languageId, DateTime useDate)
        {
            String key = buildKey(languageId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(byte languageId, DateTime useDate)
        {
            String key = buildKey(languageId, useDate);
            Cache.Remove(key);
        }
        
        private string buildKey(byte languageId, DateTime useDate)
        {
            return String.Format("BRAINTREE-ERROR-RESPONSES~{0}~{1}~{2}~{3}",
                                                                languageId,
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}