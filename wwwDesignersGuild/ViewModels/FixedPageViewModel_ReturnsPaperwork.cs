using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Returns;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_ReturnsPaperwork : FixedPageViewModel<int?>
    {
        // can't think why I'd need these: remove them if they're extraneous
        public List<Country> Countries { get; set; }
        public List<CountyItem> Counties { get; set; }

        public static async Task<FixedPageViewModel_ReturnsPaperwork> CreateAsync(
            int? orderId,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_ReturnsPaperwork(orderId,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_ReturnsPaperwork(
            int? orderId,
            BaseViewModel objBase)
            : base(orderId, objBase)
        {

        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            var db = new cmsCountry();

            #endregion

            #region The Bit in the Middle

            var countryTask =  db.countrySelectList(CountryID, LanguageID);

            await Task.WhenAll(countryTask);

            #endregion

            #region Postamble

            var countryResults = await countryTask;

            Countries = countryResults.countries;

            Counties = countryResults.counties;

            #endregion
        }
    }
}