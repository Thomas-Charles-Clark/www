using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Cache.General
{
    public class DotDigitalTrackingBehaviourCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public DotDigitalTrackingBehaviourProfile Get(short countryId, byte languageId, DateTime useDate)
        {
            return (DotDigitalTrackingBehaviourProfile)Cache[buildKey(countryId, languageId, useDate)];
        }


        public void Set(short countryId, byte languageId, DateTime useDate, DotDigitalTrackingBehaviourProfile data)
        {
            if (Boolean.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheEnabled"], out bool _))
            {
                var policy = new CacheItemPolicy();

                if (Int16.TryParse(WebConfigurationManager.AppSettings["dotDigitalBehaviourProfileCacheDuration"], out short cacheDurationMinutes))
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
            return $"DOT-DIGITAL-BEHAVIOUR-PROFILE~{countryId}~{languageId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }
}