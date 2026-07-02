using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Redirect;

namespace wwwDesignersGuild.Models.Redirects
{
    public class RedirectRepository
    {
        public async Task<RedirectDefinition> loadRedirectsFromCms(DateTime useDate)
        {
            RedirectDefinition redirectResult;

            var objCache = new RedirectCacheProvider();

            redirectResult = objCache.GetOrCreate(useDate, () => {

                var result = Task.Run(() => loadRedirectsFromCmsDatabase(useDate)).Result;

                return result;

            });

            return redirectResult;
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
}