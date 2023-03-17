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


    public class SnippetCacheProvider : ICacheSnippetProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsSnippetDefinition Get(byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, useDate);
            return (cmsSnippetDefinition)Cache[key];
        }


        public void Set(byte languageId, short countryId, DateTime useDate, cmsSnippetDefinition data)
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
            return String.Format("SNIPPET~{0}~{1}~{2}~{3}~{4}", languageId.ToString(),
                                                                        countryId.ToString(),
                                                                        useDate.Year.ToString(),
                                                                        useDate.Month.ToString(),
                                                                        useDate.Day.ToString());
        }


    }


}