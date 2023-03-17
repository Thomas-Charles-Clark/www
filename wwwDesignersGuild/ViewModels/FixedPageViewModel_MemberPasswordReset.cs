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
    public class FixedPageViewModel_MemberPasswordReset : FixedPageViewModel<PasswordResetModel>
    {
        public static async Task<FixedPageViewModel_MemberPasswordReset> CreateAsync(
            PasswordResetModel objDefinition,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberPasswordReset(
                objDefinition,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberPasswordReset(
           PasswordResetModel objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {

        }

        private async Task InitialiseAsync()
        {
            #region Preamble


            #endregion


            #region The Bit in the Middle


            #endregion


            #region Postamble


            #endregion
        }
    }
}