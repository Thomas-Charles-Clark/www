using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Redirects
{
    public class RedirectSelectResult
    {
        public string OldURL { get; set; }
        public string RedirectURL { get; set; }
        public bool Permanent { get; set; }
    }

    public class RedirectDefinition
    {
        public List<RedirectSelectResult> Redirects { get; set; }
    }

    public class RedirectRepository
    {
        public async Task<RedirectDefinition> loadRedirectsFromCms(DateTime useDate)
        {
            //  Is page in cache?
            RedirectDefinition result;
            var objCache = new RedirectCacheProvider();

            result = objCache.Get(useDate);

            if (result == null)
            {
                result = await Task.FromResult(loadRedirectsFromCmsDatabase(useDate));

                objCache.Set(useDate, result);
            }

            return result;
        }

        private RedirectDefinition loadRedirectsFromCmsDatabase(DateTime useDate)
        {
            using (var dataContext = new databaseDataContext())
            {
                var results = dataContext.Redirect_Select();

                var pageStore = new RedirectDefinition
                {
                    //  Convert from IEnumerable to a List so a collection query can be run multiple times
                    Redirects = results.ToList(),
                };

                return pageStore;
            }
        }
    }

    public class RedirectCacheProvider
    {
        private ObjectCache Cache { get { return MemoryCache.Default; } }

        public RedirectDefinition Get(DateTime useDate) => (RedirectDefinition)Cache[buildKey(useDate)];

        public void Set(DateTime useDate, RedirectDefinition data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["redirectCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["redirectCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(useDate);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }

        public bool IsSet(DateTime useDate)
        {
            String key = buildKey(useDate);
            return (Cache[key] != null);
        }

        public void Invalidate(DateTime useDate)
        {
            String key = buildKey(useDate);
            Cache.Remove(key);
        }

        private string buildKey(DateTime useDate)
        {
            return String.Format("REDIRECTS~{0}~{1}~{2}", useDate.Year.ToString(),
                                                              useDate.Month.ToString(),
                                                              useDate.Day.ToString());
        }
    }
}