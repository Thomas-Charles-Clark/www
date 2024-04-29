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
    public class ArticleSubTypeCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsArticleSubTypeDefinition Get(byte articleTypeID, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(articleTypeID, languageId, countryId, useDate);
            return (cmsArticleSubTypeDefinition)Cache[key];
        }


        public void Set(byte articleTypeId, byte languageId, short countryId, DateTime useDate, cmsArticleSubTypeDefinition data)
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

                var key = buildKey(articleTypeId, languageId, countryId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(byte articleTypeId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(articleTypeId, languageId, countryId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(byte articleTypeId, byte languageId, short countryId, DateTime useDate)
        {
            String key = buildKey(articleTypeId, languageId, countryId, useDate);
            Cache.Remove(key);
        }


        private string buildKey(byte articleTypeId, byte languageId, short countryId, DateTime useDate)
        {
            return String.Format("ARTICLE~SUB~TYPE~{0}~{1}~{2}~{3}~{4}~{5}",
                                                                articleTypeId.ToString(),
                                                                languageId.ToString(),
                                                                countryId.ToString(),
                                                                useDate.Year.ToString(),
                                                                useDate.Month.ToString(),
                                                                useDate.Day.ToString());
        }
    }
}