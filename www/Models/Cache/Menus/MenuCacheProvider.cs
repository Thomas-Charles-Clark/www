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


    public class MenuCacheProvider : ICacheMenuProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsMenuDefinition Get(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(languageId, countryId, useDate, isTrade);
            return (cmsMenuDefinition)Cache[key];
        }


        public void Set(byte languageId, short countryId, DateTime useDate, cmsMenuDefinition data, bool isTrade)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["menuCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["menuCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(languageId, countryId, useDate, isTrade);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(languageId, countryId, useDate, isTrade);
            return (Cache[key] != null);
        }


        public void Invalidate(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(languageId, countryId, useDate, isTrade);
            Cache.Remove(key);
        }


        private string buildKey(byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            return String.Format("MENU~{0}~{1}~{2}~{3}~{4}~{5}",    languageId.ToString(),
                                                                        countryId.ToString(),
                                                                        useDate.Year.ToString(),
                                                                        useDate.Month.ToString(),
                                                                        useDate.Day.ToString(),
                                                                        isTrade.ToString());
        }
    }
}