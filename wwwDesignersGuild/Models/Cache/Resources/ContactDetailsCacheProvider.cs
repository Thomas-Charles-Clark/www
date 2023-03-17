using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Cache.Resources
{
    public class ContactDetailsCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public DatabaseJSONResponse Get(int tradeAccountId, byte languageId, string departmentId)
        {
            String key = buildKey(tradeAccountId, languageId, departmentId);
            return (DatabaseJSONResponse)Cache[key];
        }


        public void Set(int tradeAccountId, byte languageId, string departmentId, DatabaseJSONResponse data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["contactDetailsCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["contactDetailsCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(tradeAccountId, languageId, departmentId);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(int tradeAccountId, byte languageId, string departmentId)
        {
            String key = buildKey(tradeAccountId, languageId, departmentId);
            return (Cache[key] != null);
        }


        public void Invalidate(int tradeAccountId, byte languageId, string departmentId)
        {
            String key = buildKey(tradeAccountId, languageId, departmentId);
            Cache.Remove(key);
        }


        private string buildKey(int tradeAccountId, byte languageId, string departmentId)
        {
            return $"CONTACT-DETAILS~{tradeAccountId}~{languageId}~{departmentId}";
        }
    }
}