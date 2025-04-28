using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Results;

using Braintree;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_MemberUnsubscribe : FixedPageViewModel<cmsUnsubscribePageDefinition>
    {
        public string UnsubGuid { get; set; }
        public string State { get; set; }
        public Guid? Guid { get; set; }
        public MemberDetail Member { get; set; }
        public string EmailName { get; set; }

        public static async Task<FixedPageViewModel_MemberUnsubscribe> CreateAsync(
            cmsUnsubscribePageDefinition objDefinition,
            string unsub,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_MemberUnsubscribe(
                objDefinition,
                unsub,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_MemberUnsubscribe(
           cmsUnsubscribePageDefinition objDefinition,
           string unsub,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {
            UnsubGuid = unsub;
        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            var resourceDefinition = new cmsResource().loadResourceFromCms(
                    LanguageID,
                    CountryID,
                    Date).Result;

            var resources = new List<ResourceResult>();

            var required = new [] {
                "UnsubscribeHeadingSorry",
                "UnsubscribeHeadingYou'reThinking",
                "UnsubscribeLessFrequentTouching",
                "UnsubscribeOnceAWeek",
                "UnsubscribeToSayThankYou",
                "UnsubscribeSoundsGood",
                "UnsubscribeSendMeMoreSelective",
                "UnsubscribeNoThanks",
                "UnsubscribeI'dLikeToUnsubscribe"
            };

            foreach (var r in required)
            {
                resources.Add(new ResourceResult()
                {
                    Name = r,
                    Value = resourceDefinition.resourceSelects
                                            .Where(p => p.Name == r)
                                            .Select(p => p.Value)
                                            .SingleOrDefault()
                });
            }

            this.BaseViewModel.PrefetchedResources.AddRange(resources);

            #endregion


            #region The Bit in the Middle

            if (System.Guid.TryParse(UnsubGuid, out Guid memberGuid))
            {
                var factory = new VisitorFactory();

                var member = await factory.getMemberByGuid(memberGuid);

                if (member == null)
                {
                    Member = null;
                    Guid = memberGuid;
                    State = "memberNotFound";
                }

                Member = member;
                Guid = memberGuid;
                State = "memberFound";
            }
            else
            {
                Member = null;
                Guid = null;
                State = "failedGUID";
            }

            #endregion


            #region Postamble


            #endregion
        }
    }
}