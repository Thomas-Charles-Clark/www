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


    public class CollectionProductPageCacheProvider : ICacheCollectionProductPageProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public cmsCollectionProductPageDefinition Get(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);
            return (cmsCollectionProductPageDefinition)Cache[key];
        }


        public void Set(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort, cmsCollectionProductPageDefinition data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["productPageCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["productPageCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);
            return (Cache[key] != null);
        }


        public void Invalidate(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort)
        {
            String key = buildKey(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);
            Cache.Remove(key);
        }


        private string buildKey(int collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort)
        {
            String brand = "";
            if (brandId == null) { }
            else
            {
                brand = brandId.ToString();
            }
            String productType = "";
            if (productTypeId == null) { }
            else
            {
                productType = productTypeId.ToString();
            }
            if (sort == null)
            {
                sort = "";
            }
            return String.Format("COLLECTIONPRODUCT~{0}~{1}~{2}~{3}~{4}~{5}~{6}~{7}~{8}", collectionId.ToString(),
                                                                                            languageId.ToString(),
                                                                                            countryId.ToString(),
                                                                                            useDate.Year.ToString(),
                                                                                            useDate.Month.ToString(),
                                                                                            useDate.Day.ToString(),
                                                                                            brand,
                                                                                            productType,
                                                                                            sort);
        }


    }


}