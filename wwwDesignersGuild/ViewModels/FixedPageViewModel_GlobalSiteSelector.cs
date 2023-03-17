using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Util;       

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_GlobalSiteSelector : FixedPageViewModel<object>
    {
        //public List<Country> Countries { get; set; }

        //public List<Language> Languages { get; set; }

        public string UserCountryCode { get; set; }

        public byte? PreSelectedCountryLanguageID { get; set; }

        public List<countryItem> Sites { get; set; }

        protected FixedPageViewModel_GlobalSiteSelector(
            string userCountryCode,
            BaseViewModel objBase)
            : base(new object(), objBase)
        {
            UserCountryCode = userCountryCode?.ToUpperInvariant();
        }

        public static async Task<FixedPageViewModel_GlobalSiteSelector> CreateAsync(string userCountryCode, BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_GlobalSiteSelector(userCountryCode, objBase);

            await result.Initialise();

            return result;
        }

        new protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif


            // HttpRequestWrapper - for pulling country / date overrides
            var objRequestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

            // Date to use
            var useDate = Framework.GetUseDate(objRequestWrapper);

            var cmsCountry = new cmsCountry();

            var countryLoad = cmsCountry.countryLanguageSelect(useDate);

            var languageLoad = cmsCountry.loadCompleteLanguageList(MasterDefinition.UrlBreakdown.countryDetails.CountryID, useDate);

            await Task.WhenAll(countryLoad, languageLoad);

            var countryResults = await countryLoad;

            var languageResults = await languageLoad;

            Sites = countryResults.countryItems;

            PreSelectedCountryLanguageID = Sites.FirstOrDefault(s => s.CountryCode == UserCountryCode)?.CountryLanguageID;

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_GlobalSiteSelector initialised in {sw.Elapsed}");
#endif
        }
    }
}