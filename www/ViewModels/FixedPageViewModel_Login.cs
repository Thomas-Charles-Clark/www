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
    public class FixedPageViewModel_Login : FixedPageViewModel<object>
    {
        public List<Country> Countries { get; set; }

        public List<Language> Languages { get; set; }

        public string PrivacyURL
        {
            get
            {
                return dgFramework.buildUrl(MasterDefinition.UrlBreakdown.countryDetails,
                    null,
                    dgContentType.landingPage,
                    (int)dgFixedPage.privacyPolicy);
            }
        }

        public string CookieURL
        {
            get
            {
                return dgFramework.buildUrl(MasterDefinition.UrlBreakdown.countryDetails,
                    null,
                    dgContentType.landingPage,
                    (int)dgFixedPage.cookie);
            }
        }

        public string TermsURL
        {
            get
            {
                return dgFramework.buildUrl(MasterDefinition.UrlBreakdown.countryDetails,
                    null,
                    dgContentType.landingPage,
                    (int)dgFixedPage.termsAndConditions);
            }
        }

        public bool BeginRegistration { get; set; }

        public string Redirect { get; set; }

        protected FixedPageViewModel_Login(
            BaseViewModel objBase)
            : base(new object(), objBase)
        {
            BeginRegistration = false;
        }

        public static async Task<FixedPageViewModel_Login> CreateAsync(BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_Login(objBase);

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

            var countryLoad = cmsCountry.countrySelectList(null, null);

            var languageLoad = cmsCountry.loadCompleteLanguageList(MasterDefinition.UrlBreakdown.countryDetails.CountryID, useDate);

            await Task.WhenAll(countryLoad, languageLoad);

            var countryResults = await countryLoad;

            var languageResults = await languageLoad;

            Countries = countryResults.countries;

            Languages = languageResults.languages;

            BeginRegistration = HttpContext.Current.Request.QueryString["r"] != null;

            var autoAddPromoCode = !String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["a"]);

            var kvps = new List<KeyValuePair<string, string>>();

            if (autoAddPromoCode)
            {
                kvps.Add(new KeyValuePair<string, string>("a", HttpContext.Current.Request.QueryString["a"]));
            }

            if (Int32.TryParse(HttpContext.Current.Request.QueryString["redirect"], out int redirectContentId))
            {
                var r = HttpContext.Current.Request.QueryString["contentType"] == null 
                    ? "l" 
                    : HttpContext.Current.Request.QueryString["contentType"].ToString();

                var contentType = dgFramework.getContentTypeFromString(r);

                Redirect = dgFramework.buildUrl(
                    contentType,
                    redirectContentId,
                    MasterDefinition.UrlBreakdown.countryDetails,
                    null,
                    null,
                    kvps,
                    null,
                    null);
            }

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_Login initialised in {sw.Elapsed}");
#endif
        }
    }
}