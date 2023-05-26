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
    public class FilterIndexAttributeCombination
    {
        public byte ProductTypeID { get; set; }
        public string AttributeSignature { get; set; }
        public string AttributeDescription { get; set; }
        public string FooterText { get; set; }
    }

    public class cmsFilterIndexAttributeCombinationResult
    {
        public IEnumerable<FilterIndexAttributeCombination> Combinations { get; set; }
    }

    public class PassFilterIndexAttributeCombinationCacheFetch
    {
        public byte LanguageID { get; set; }
        public DateTime Date { get; set; }
    }

    public class cmsFilterIndexAttributeCombination
    {
        public async Task<cmsFilterIndexAttributeCombinationResult> loadFilterIndexingAttributeCombinationsFromCmsAsync(PassFilterIndexAttributeCombinationCacheFetch
            objFetch, BaseViewModel objBase)
        {
            // Is page in cache?
            var cache = new FilterIndexAttributeCombinationCacheProvider();
            var data = cache.Get(objFetch);

            if (data == null)
            {
                data = await loadFilterIndexingAttributeCombinationsFromCms(objFetch, objBase);

                cache.Set(objFetch, data);
            }

            return data;
        }

        private async Task<cmsFilterIndexAttributeCombinationResult> loadFilterIndexingAttributeCombinationsFromCms(PassFilterIndexAttributeCombinationCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = await new databaseDataContext().FetchFilterIndexAttributeCombinationsAsync(objFetch.LanguageID);

            return new cmsFilterIndexAttributeCombinationResult()
            {
                Combinations = results
            };
        }

        public cmsFilterIndexAttributeCombinationResult loadFilterIndexingRestrictionsFromCmsSync(PassFilterIndexAttributeCombinationCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = new databaseDataContext().FetchFilterIndexAttributeCombinations(objFetch.LanguageID);

            return new cmsFilterIndexAttributeCombinationResult()
            {
                Combinations = results
            };
        }
    }
}