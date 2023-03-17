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
    public class LandingPageCacheProvider : ICacheLandingPageProvider
    {

        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsLandingPageDefinition Get(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(pageId, languageId, countryId, useDate, isTrade);
            return (cmsLandingPageDefinition)Cache[key];
        }


        public void Set(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade, cmsLandingPageDefinition data)
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

                var key = buildKey(pageId, languageId, countryId, useDate, isTrade);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(pageId, languageId, countryId, useDate, isTrade);
            return (Cache[key] != null);
        }


        public void Invalidate(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(pageId, languageId, countryId, useDate, isTrade);
            Cache.Remove(key);
        }


        private string buildKey(int pageId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            return String.Format("PAGE~{0}~{1}~{2}~{3}~{4}~{5}~{6}", pageId.ToString(), 
                                                                languageId.ToString(), 
                                                                countryId.ToString(), 
                                                                isTrade,
                                                                useDate.Year.ToString(), 
                                                                useDate.Month.ToString(), 
                                                                useDate.Day.ToString());
        }
    }



    public class SiteSectionCacheProvider
    {

        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public byte? Get(int pageId, DateTime useDate)
        {
            String key = buildKey(pageId, useDate);
            return (byte?)Cache[key];
        }


        public void Set(int pageId, DateTime useDate, byte? data)
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

                var key = buildKey(pageId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int pageId, DateTime useDate)
        {
            String key = buildKey(pageId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(int pageId, DateTime useDate)
        {
            String key = buildKey(pageId, useDate);
            Cache.Remove(key);
        }


        private string buildKey(int pageId, DateTime useDate)
        {
            return String.Format("SITE-SECTION~{0}~{1}~{2}~{3}", pageId.ToString(),
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}