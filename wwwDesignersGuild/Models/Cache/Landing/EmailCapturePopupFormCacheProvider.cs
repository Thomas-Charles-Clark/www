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
    public class EmailCapturePopupFormCacheProvider : ICacheEmailCapturePopupFormProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public cmsEmailCapturePopupForm Get(short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(countryId, languageId, useDate);
            return (cmsEmailCapturePopupForm)Cache[key];
        }

        public void Set(short countryId, byte languageId, DateTime useDate, cmsEmailCapturePopupForm data)
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

                var key = buildKey(countryId, languageId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public bool IsSet(short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(countryId, languageId, useDate);
            return (Cache[key] != null);
        }

        public void Invalidate(short countryId, byte languageId, DateTime useDate)
        {
            String key = buildKey(countryId, languageId, useDate);
            Cache.Remove(key);
        }

        private string buildKey(short countryId, byte languageId, DateTime useDate)
        {
            return String.Format("EMAIL~CAPUTRE~POPUP~FORM~{0}~{1}~{2}~{3}~{4}",
                                                                countryId.ToString(),
                                                                languageId.ToString(),
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}