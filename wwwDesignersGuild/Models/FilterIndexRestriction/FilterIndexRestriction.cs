using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Database;

using wwwDesignersGuild.Models.Hotspot;

using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models
{
    public class FilterIndexRestriction
    {
        public byte ProductTypeID { get; set; }
        public string AttributeGroupSignature { get; set; }
        public string AttributeGroupDescription { get; set; }
        public bool Deleted { get; set; }
    }

    public class cmsFilterIndexRestrictionResult
    {
        public IEnumerable<FilterIndexRestriction> Restrictions { get; set; }
    }

    public class PassFilterIndexRestrictionCacheFetch
    {
        public DateTime Date { get; set; }
    }

    public class cmsFilterIndexRestriction
    {
        public async Task<cmsFilterIndexRestrictionResult> loadFilterIndexingRestrictionsFromCmsAsync(PassFilterIndexRestrictionCacheFetch
            objFetch, BaseViewModel objBase)
        {
            // Is page in cache?
            var cache = new FilterIndexRestrictionCacheProvider();
            var data = cache.Get(objFetch);

            if (data == null)
            {
                data = await loadFilterIndexingRestrictionsFromCms(objFetch, objBase);

                cache.Set(objFetch, data);
            }

            return data;
        }

        private async Task<cmsFilterIndexRestrictionResult> loadFilterIndexingRestrictionsFromCms(PassFilterIndexRestrictionCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = await new databaseDataContext().FetchFilterIndexRestrictionsAsync();

            return new cmsFilterIndexRestrictionResult()
            {
                Restrictions = results
            };
        }

        public cmsFilterIndexRestrictionResult loadFilterIndexingRestrictionsFromCmsSync(PassFilterIndexRestrictionCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = new databaseDataContext().FetchFilterIndexRestrictions();

            return new cmsFilterIndexRestrictionResult()
            {
                Restrictions = results
            };
        }
    }
}