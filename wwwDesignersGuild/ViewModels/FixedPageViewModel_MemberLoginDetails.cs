using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Form;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_MemberLoginDetails : FixedPageViewModel<MemberDetail>
    {
        public MarketingPermissionResult MarketingPermissions { get; set; }
        public List<Country> Countries { get; set; }
        public List<Language> Languages { get; set; }

        public static async Task<FixedPageViewModel_MemberLoginDetails> CreateAsync(
            MemberDetail objDefinition,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberLoginDetails(objDefinition,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberLoginDetails(
            MemberDetail objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {

        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            var db = new cmsCountry();
            var formDb = new formRoutines();

            #endregion

            #region The Bit in the Middle

            var marketingPermissionsTask = formDb.marketingPermissionCheck(new webapiMarketingPermissionCheck()
            {
                VisitorID = Visitor.visitorDbId,
                MemberID = Visitor.memberId,
                Email = Definition.Email,
                AddressRequired = true,
                CountryID = CountryID,
                LanguageID = LanguageID,
                SourceID = 50,
                MarketingCountryID = Definition.MarketingCountryID,
                ThirdPartyID = null
            });

            var countryTask =  db.countrySelectList(null, null);
            var languageTask = db.loadCompleteLanguageList(null, Date);

            await Task.WhenAll(countryTask, languageTask, marketingPermissionsTask);

            #endregion

            #region Postamble

            var marketingPermissionResults = await marketingPermissionsTask;

            var countryResults = await countryTask;

            var languageResults = await languageTask;

            MarketingPermissions = marketingPermissionResults;

            Countries = countryResults.countries;

            Languages = languageResults.languages;

            #endregion
        }
    }
}