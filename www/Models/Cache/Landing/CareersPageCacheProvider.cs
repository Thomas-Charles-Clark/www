using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Careers;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Cache
{
    public class CareersPageCacheProvider : ICacheCareersPageProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public Careers.cmsCareersPageDefinition Get(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (cmsCareersPageDefinition)Cache[key];
        }


        public void Set(byte languageId, short countryId, DateTime useDate, cmsCareersPageDefinition data)
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
            return String.Format("CAREERS~{0}~{1}~{2}~{3}~{4}", 
                                                                languageId.ToString(),
                                                                countryId.ToString(),
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }


    }
}