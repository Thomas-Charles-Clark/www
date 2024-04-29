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
    public class LifestyleGalleryPopupCacheProvider : ICacheLifestyleGalleryPopupProvider
    {

        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsLifestyleGalleryPopupDefinition Get(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, useDate);
            return (cmsLifestyleGalleryPopupDefinition)Cache[key];
        }


        public void Set(int productPageId, byte languageId, short countryId, DateTime useDate, cmsLifestyleGalleryPopupDefinition data)
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

                var key = buildKey(productPageId, languageId, countryId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(productPageId, languageId, countryId, useDate);
            Cache.Remove(key);
        }


        private string buildKey(int productPageId, byte languageId, short countryId, DateTime useDate)
        {
            return String.Format("LIFESTYLEGP~{0}~{1}~{2}~{3}~{4}~{5}", productPageId.ToString(),
                                                                        languageId.ToString(),
                                                                        countryId.ToString(),
                                                                        useDate.Year.ToString(),
                                                                        useDate.Month.ToString(),
                                                                        useDate.Day.ToString());
        }
    }
}