using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Search
{
    public class SearchCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public serverSearchDatabase Get(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);
            return (serverSearchDatabase)Cache[key];
        }

        public void Set(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate, serverSearchDatabase data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["searchCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["searchCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public void ExplicitSet(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate, serverSearchDatabase data, DateTimeOffset expiration)
        {
            CacheItemPolicy policy = new CacheItemPolicy
            {
                AbsoluteExpiration = expiration
            };

            String key = buildKey(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);
            Cache.Add(new CacheItem(key, data), policy);
        }

        public bool IsSet(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);
            return (Cache[key] != null);
        }


        public void Invalidate(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate)
        {
            String key = buildKey(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);
            Cache.Remove(key);
        }


        private string buildKey(byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate)
        {
            return $"SEARCHCACHE~{languageId}~{countryId}~{searchTerm}~{tradeAccountId}~{productTypeId}~{useDate.Year}~{useDate.Month}~{useDate.Day}";
        }
    }


    public class cmsSearch
    {
        public async Task<serverSearchDatabase> loadSearchFromCms(DataTable productPageTable, byte languageId, short countryId, string searchTerm, int? tradeAccountId, byte? productTypeId, DateTime useDate)
        {
            //  Is result already in cache?
            serverSearchDatabase result = null;

            var cache = new SearchCacheProvider();

            result = cache.Get(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate);

            if (result == null)
            {
                result = await Task.FromResult<serverSearchDatabase>(loadSearchFromDatabase(productPageTable, languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate));

                cache.Set(languageId, countryId, searchTerm, tradeAccountId, productTypeId, useDate, result);
            }

            return result;
        }

        private serverSearchDatabase loadSearchFromDatabase(
            DataTable productPageTable, 
            byte languageId, 
            short countryId, 
            string searchTerm, 
            int? tradeAccountId,
			byte? productTypeId,
			DateTime useDate)
        {
            var objDbSearchResults = new searchDb();
            return objDbSearchResults.generateServerSearchResults(productPageTable,
                languageId,
                countryId,
                useDate,
                String.IsNullOrEmpty(searchTerm),
                tradeAccountId,
                tradeAccountId.HasValue,
				productTypeId);
        }
    }
}