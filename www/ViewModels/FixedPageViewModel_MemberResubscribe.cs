using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Form;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_MemberResubscribe : FixedPageViewModel<object>
    {
        public string Resub { get; set; }
        public Guid? Guid { get; set; }
        public MemberDetail Member { get; set; }

        public static async Task<FixedPageViewModel_MemberResubscribe> CreateAsync(
            string unsub,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberResubscribe(
                unsub,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberResubscribe(
            string resub,
            BaseViewModel objBase)
            : base(new object(), objBase)
        {
            Resub = resub;
        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            Task<MemberDetail> memberTask = null;

            var visitoryFactory = new VisitorFactory();

            if (!String.IsNullOrEmpty(Resub))
            {
                if (System.Guid.TryParse(Resub, out Guid g))
                {
                    Guid = g;
                    memberTask = visitoryFactory.getMemberByGuid(g);
                }
            }

            #endregion


            #region The Bit in the Middle

            if (memberTask != null)
            {
                await Task.WhenAll(memberTask);

                Member = await memberTask;
            }

            #endregion


            #region Postamble
            
            if (Member != null && Member.Unsubscribed)
            {
                var objFormRoutines = new formRoutines();

                _ = await objFormRoutines.resubscribeMember(Guid.Value);
            }

            #endregion
        }
    }
}