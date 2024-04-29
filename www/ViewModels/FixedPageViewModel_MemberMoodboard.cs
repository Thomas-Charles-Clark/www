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
    public class FixedPageViewModel_MemberMoodboard : FixedPageViewModel<MoodboardContainer>
    {
        public static async Task<FixedPageViewModel_MemberMoodboard> CreateAsync(
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberMoodboard(
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberMoodboard(
            BaseViewModel objBase)
            : base(objBase)
        {

        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            var vf = new VisitorFactory();

            #endregion

            #region The Bit in the Middle

            var results = await vf.getMemberMoodboards(
                Visitor.memberId.Value,
                CountryID,
                LanguageID,
                Date
            );

            #endregion

            #region Postamble

            Definition = results;

            #endregion
        }
    }
}