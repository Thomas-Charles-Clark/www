using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_MemberAddress : FixedPageViewModel<MemberAddressSelect>
    {
        public List<Country> Countries { get; set; }
        public List<CountyItem> Counties { get; set; }
        public MemberAddress DefaultMemberAddress { get; set; }

        public static async Task<FixedPageViewModel_MemberAddress> CreateAsync(
            MemberAddressSelect objDefinition,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberAddress(objDefinition,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberAddress(
            MemberAddressSelect objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {

        }

        protected async Task InitialiseAsync()
        {
            #region Preamble

            var db = new cmsCountry();

            #endregion

            #region The Bit in the Middle

            var countryTask =  db.countrySelectList(CountryID, LanguageID);

            var defaultAddressTask = new VisitorFactory().getDefaultMemberAddress(Visitor.memberId.Value, CountryID);

            await Task.WhenAll(countryTask, defaultAddressTask);

            #endregion

            #region Postamble

            var countryResults = await countryTask;

            Countries = countryResults.countries;

            Counties = countryResults.counties;

            var defaultAddressResult = await defaultAddressTask;

            DefaultMemberAddress = defaultAddressResult;

            #endregion
        }
    }
}