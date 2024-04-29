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


    public class CollectionPageCacheProvider : ICacheCollectionPageProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsCollectionPageDefinition Get(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, isTrade);
            return (cmsCollectionPageDefinition)Cache[key];
        }


        public void Set(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade, cmsCollectionPageDefinition data)
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

                var key = buildKey(collectionId, languageId, countryId, useDate, isTrade);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, isTrade);
            return (Cache[key] != null);
        }


        public void Invalidate(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, isTrade);
            Cache.Remove(key);
        }


        private string buildKey(int collectionId, byte languageId, short countryId, DateTime useDate, bool isTrade)
        {
            return String.Format("COLLECTION~{0}~{1}~{2}~{3}~{4}~{5}", collectionId.ToString(),
                                                                        languageId.ToString(),
                                                                        countryId.ToString(),
                                                                        useDate.Year.ToString(),
                                                                        useDate.Month.ToString(),
                                                                        useDate.Day.ToString(),
                                                                        isTrade.ToString());
        }


    }


}
