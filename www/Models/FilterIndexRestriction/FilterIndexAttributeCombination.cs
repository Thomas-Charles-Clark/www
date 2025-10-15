using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
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
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public bool Enabled { get; set; }
    }

    public class cmsFilterIndexAttributeCombinationResult
    {
        public IEnumerable<FilterIndexAttributeCombination> Combinations { get; set; }
    }

    public class PassFilterIndexAttributeCombinationCacheFetch
    {
        public byte LanguageID { get; set; }
        public byte? ProductTypeID { get; set; }
        public DateTime Date { get; set; }
        public string CountryURL { get; set; }

        public PassFilterIndexAttributeCombinationCacheFetch(byte languageID, byte? productTypeID, DateTime date, string countryURL)
        {
            LanguageID = languageID;
            ProductTypeID = productTypeID;
            Date = date;
            CountryURL = countryURL;
        }
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
            var results = await new databaseDataContext().FetchFilterIndexAttributeCombinationsAsync(objFetch.LanguageID, objFetch.ProductTypeID, objBase.CountryURL);

            foreach (var item in results)
            {
                if (!String.IsNullOrEmpty(item.FooterText))
                {
                    item.FooterText = HtmlHelperExtra.ProcessHTMLImages(item.FooterText, null, new long[] { }, new ImageHotspot[] { }, objBase);
                }
            }

            return new cmsFilterIndexAttributeCombinationResult()
            {
                Combinations = results
            };
        }

        public cmsFilterIndexAttributeCombinationResult loadFilterIndexingRestrictionsFromCmsSync(PassFilterIndexAttributeCombinationCacheFetch objFetch, BaseViewModel objBase)
        {
            var results = new databaseDataContext().FetchFilterIndexAttributeCombinations(objFetch.LanguageID, objBase.CountryURL);

            foreach (var item in results)
            {
                if (!String.IsNullOrEmpty(item.FooterText))
                {
                    item.FooterText = HtmlHelperExtra.ProcessHTMLImages(item.FooterText, null, new long[] { }, new ImageHotspot[] { }, objBase);
                }
            }

            return new cmsFilterIndexAttributeCombinationResult()
            {
                Combinations = results
            };
        }
    }
}